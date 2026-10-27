using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Terraria.Diagnostics;
using Terraria.Liquid;
using Terraria.World;

namespace Terraria.Communication;

/// <summary>
/// Owns the bounded Terraria Version4 admission exchange at the ECS communication boundary.
///
/// The ECS gameplay adapter intentionally only understands gameplay packets. This adapter keeps
/// the transport protocol state machine in front of it, emits the minimum legal join responses,
/// and delegates packets only after the client reaches the Active stage.
/// </summary>
public sealed class TerrariaProtocolAdmissionHandler : ICommunicationPacketHandler
{
  private const byte HelloMessageId = 1;
  private const byte PlayerInfoMessageId = 3;
  private const byte SyncPlayerMessageId = 4;
  private const byte RequestWorldDataMessageId = 6;
  private const byte WorldDataMessageId = 7;
  private const byte SpawnTileDataMessageId = 8;
  private const byte StatusTextSizeMessageId = 9;
  private const byte InitialSpawnMessageId = 49;
  private const byte PlayerSpawnMessageId = 12;
  private const byte TileSectionMessageId = 10;
  private const byte RequestSectionMessageId = 159;
  private const byte FinishedConnectingMessageId = 129;

  private readonly ICommunicationPacketHandler _gameplay;
  private readonly TerrariaProtocolOptions _options;
  private readonly IReadOnlyList<PreparedTileSection> _initialSections;
  private readonly IReadOnlyDictionary<(int X, int Y), PreparedTileSection> _sections;
  private readonly object _sectionGate = new();
  private readonly Dictionary<SessionEpoch, HashSet<(int X, int Y)>> _sentSections = [];

  public TerrariaProtocolAdmissionHandler(
      ICommunicationPacketHandler gameplay,
      TerrariaProtocolOptions? options = null)
  {
    _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
    _options = options ?? new TerrariaProtocolOptions();
    _options.Validate();
    _sections = PrepareInitialSections(out _initialSections);
  }

  public IReadOnlyList<OutboundPacketIntent> Handle(
      InboundCommunicationPacket packet,
      CommunicationSessionComponent session,
      long tick)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(session);
    if (packet.Envelope.Epoch != session.Epoch || tick < 0)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    if (session.Stage is CommunicationSessionStage.Closing or CommunicationSessionStage.Closed)
    {
      lock (_sectionGate)
      {
        _sentSections.Remove(session.Epoch);
      }
    }

    byte messageId = packet.Envelope.MessageId;
    try
    {
      return session.Stage switch
      {
        CommunicationSessionStage.Connected =>
            HandleHello(packet.Frame.Payload.Span, session),
        CommunicationSessionStage.AwaitPlayerData =>
            HandlePlayerData(packet.Frame.Payload.Span, messageId, session),
        CommunicationSessionStage.AwaitSectionRequest =>
            HandleSectionRequest(packet.Frame.Payload.Span, messageId, session),
        CommunicationSessionStage.Synchronizing =>
            HandleSynchronizing(packet.Frame.Payload.Span, messageId, session),
        CommunicationSessionStage.Active => messageId == RequestSectionMessageId
            ? HandleSectionRequest(packet.Frame.Payload.Span, session)
            : _gameplay.Handle(packet, session, tick),
        _ => Array.Empty<OutboundPacketIntent>()
      };
    }
    catch (ArgumentException exception)
    {
      ServerLog.Warning(
          "ecs.protocol.rejected",
          $"epoch={session.Epoch.Value} messageId={messageId} detail={ServerLog.Quote(exception.Message)}");
      return Array.Empty<OutboundPacketIntent>();
    }
  }

  private IReadOnlyList<OutboundPacketIntent> HandleHello(
      ReadOnlySpan<byte> payload,
      CommunicationSessionComponent session)
  {
    string version = ReadString(payload);
    if (!IsCompatibleVersion(version))
    {
      ServerLog.Warning(
          "ecs.protocol.version_rejected",
          $"epoch={session.Epoch.Value} version={ServerLog.Quote(version)}");
      return Array.Empty<OutboundPacketIntent>();
    }

    if (!session.TryAdvance(CommunicationSessionStage.AwaitPlayerData))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info("ecs.protocol.player_info", $"epoch={session.Epoch.Value} slot=0");
    return [CreateIntent(PlayerInfoMessageId, [0, 0], session)];
  }

  private bool IsCompatibleVersion(string version)
  {
    if (string.Equals(version, _options.ProtocolVersion, StringComparison.Ordinal))
    {
      return true;
    }

    if (!_options.AcceptCompatibleTerrariaVersions ||
        !version.StartsWith("Terraria", StringComparison.Ordinal) ||
        version.Length == "Terraria".Length)
    {
      return false;
    }

    for (int index = "Terraria".Length; index < version.Length; index++)
    {
      if (!char.IsAsciiDigit(version[index]))
      {
        return false;
      }
    }

    return true;
  }

  private IReadOnlyList<OutboundPacketIntent> HandlePlayerData(
      ReadOnlySpan<byte> payload,
      byte messageId,
      CommunicationSessionComponent session)
  {
    // Packet 4 (player appearance) is uploaded before packet 6. Its complete authoritative
    // player projection belongs to the gameplay/player system; admission only consumes it.
    if (messageId == SyncPlayerMessageId)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    if (messageId != RequestWorldDataMessageId || payload.Length != 0 ||
        !session.TryAdvance(CommunicationSessionStage.AwaitSectionRequest))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info("ecs.protocol.world_data", $"epoch={session.Epoch.Value}");
    return [CreateIntent(WorldDataMessageId, CreateWorldDataPayload(), session)];
  }

  private IReadOnlyList<OutboundPacketIntent> HandleSectionRequest(
      ReadOnlySpan<byte> payload,
      byte messageId,
      CommunicationSessionComponent session)
  {
    if (messageId != SpawnTileDataMessageId || payload.Length != 9 ||
        !session.TryAdvance(CommunicationSessionStage.Synchronizing))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info("ecs.protocol.initial_spawn", $"epoch={session.Epoch.Value}");
    // Terraria's Version4 section grid is 200x150. Keeping the protocol-native height is
    // important for join latency: a 50-tile height triples the number of compressed streams the
    // client must inflate and validate before it can send packet 12 (2016 sections for this
    // world instead of 672).
    int sectionCount = _initialSections.Count;
    var responses = new List<OutboundPacketIntent>(sectionCount + 2)
    {
      // MinimalImplementation repeats packet 7 immediately before section transfer. Terraria
      // uses this second world header as the synchronization baseline for packet 10.
      CreateIntent(WorldDataMessageId, CreateWorldDataPayload(), session),
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(sectionCount), session)
    };
    foreach (PreparedTileSection section in _initialSections)
    {
      TryMarkSectionSent(session.Epoch, section.SectionX, section.SectionY);
      responses.Add(CreateIntent(TileSectionMessageId, section.Payload, session));
    }

    responses.Add(CreateIntent(InitialSpawnMessageId, Array.Empty<byte>(), session));
    return responses.AsReadOnly();
  }

  private IReadOnlyDictionary<(int X, int Y), PreparedTileSection> PrepareInitialSections(
      out IReadOnlyList<PreparedTileSection> initialSections)
  {
    const int sectionHeight = 150;
    long started = Stopwatch.GetTimestamp();
    var sections = new List<PreparedTileSection>(
        ((_options.WorldWidth + 199) / 200) *
        ((_options.WorldHeight + sectionHeight - 1) / sectionHeight));
    long totalBytes = 0;
    for (int startY = 0; startY < _options.WorldHeight; startY += sectionHeight)
    {
      int height = Math.Min(sectionHeight, _options.WorldHeight - startY);
      for (int startX = 0; startX < _options.WorldWidth; startX += 200)
      {
        int width = Math.Min(200, _options.WorldWidth - startX);
        byte[] payload = CreateTileSectionPayload(startX, startY, width, height);
        sections.Add(new PreparedTileSection(startX / 200, startY / sectionHeight, payload));
        totalBytes += payload.Length;
      }
    }

    ServerLog.Info(
        "ecs.protocol.world_cache_ready",
        $"sections={sections.Count} compressedBytes={totalBytes} " +
        $"elapsedMs={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F0}");
    IReadOnlyList<PreparedTileSection> prepared = Array.AsReadOnly(sections.ToArray());
    initialSections = SelectInitialSections(prepared, _options.SpawnTileX, _options.SpawnTileY);
    return prepared.ToDictionary(section => (section.SectionX, section.SectionY));
  }

  private static IReadOnlyList<PreparedTileSection> SelectInitialSections(
      IReadOnlyList<PreparedTileSection> sections, int spawnTileX, int spawnTileY)
  {
    int centerX = spawnTileX / 200;
    int centerY = spawnTileY / 150;
    return sections.Where(section =>
        Math.Abs(section.SectionX - centerX) <= 2 &&
        Math.Abs(section.SectionY - centerY) <= 1).ToArray();
  }

  private IReadOnlyList<OutboundPacketIntent> HandleSynchronizing(
      ReadOnlySpan<byte> payload,
      byte messageId,
      CommunicationSessionComponent session)
  {
    if (messageId == RequestSectionMessageId)
    {
      return HandleSectionRequest(payload, session);
    }

    return HandleSpawn(payload, messageId, session);
  }

  private IReadOnlyList<OutboundPacketIntent> HandleSectionRequest(
      ReadOnlySpan<byte> payload,
      CommunicationSessionComponent session)
  {
    if (payload.Length != 4 || !session.IsConnected)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    int sectionX = BinaryPrimitives.ReadUInt16LittleEndian(payload);
    int sectionY = BinaryPrimitives.ReadUInt16LittleEndian(payload[2..]);
    if (!_sections.TryGetValue((sectionX, sectionY), out PreparedTileSection? section) ||
        !TryMarkSectionSent(session.Epoch, sectionX, sectionY))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    return [
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(1), session),
      CreateIntent(TileSectionMessageId, section.Payload, session)
    ];
  }

  private bool TryMarkSectionSent(SessionEpoch epoch, int sectionX, int sectionY)
  {
    lock (_sectionGate)
    {
      if (!_sentSections.TryGetValue(epoch, out HashSet<(int X, int Y)>? sent))
      {
        sent = [];
        _sentSections.Add(epoch, sent);
      }

      return sent.Add((sectionX, sectionY));
    }
  }

  private static IReadOnlyList<OutboundPacketIntent> HandleSpawn(
      ReadOnlySpan<byte> payload,
      byte messageId,
      CommunicationSessionComponent session)
  {
    if (messageId != PlayerSpawnMessageId || payload.Length < 1 ||
        !session.TryAdvance(CommunicationSessionStage.Active))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info("ecs.protocol.active", $"epoch={session.Epoch.Value}");
    return [CreateIntent(FinishedConnectingMessageId, Array.Empty<byte>(), session)];
  }

  private byte[] CreateWorldDataPayload()
  {
    using var stream = new MemoryStream();
    WriteInt32(stream, _options.WorldTime);
    stream.WriteByte(_options.DayTime ? (byte)1 : (byte)0);
    stream.WriteByte(_options.MoonPhase);
    WriteInt16(stream, checked((short)_options.WorldWidth));
    WriteInt16(stream, checked((short)_options.WorldHeight));
    WriteInt16(stream, checked((short)_options.SpawnTileX));
    WriteInt16(stream, checked((short)_options.SpawnTileY));
    WriteInt16(stream, _options.WorldSurface);
    WriteInt16(stream, _options.RockLayer);
    WriteInt32(stream, _options.WorldId);
    WriteString(stream, _options.WorldName);
    stream.WriteByte(checked((byte)_options.GameMode));
    stream.Write((_options.WorldGuid == Guid.Empty ? new byte[16] : _options.WorldGuid.ToByteArray()));
    WriteUInt64(stream, _options.WorldGeneratorVersion);
    stream.WriteByte(0);
    stream.Write(new byte[13]);
    stream.Write(new byte[3]);
    WriteSingle(stream, 0);
    stream.WriteByte(0);
    WriteInt32(stream, 0);
    WriteInt32(stream, 0);
    WriteInt32(stream, 0);
    stream.Write(new byte[4]);
    WriteInt32(stream, 0);
    WriteInt32(stream, 0);
    WriteInt32(stream, 0);
    stream.Write(new byte[4]);
    stream.Write(new byte[13]);
    WriteSingle(stream, 0);
    stream.Write(new byte[11]);
    stream.WriteByte(0);
    stream.WriteByte(0);
    for (int i = 0; i < 7; i++) WriteInt16(stream, 0);
    stream.WriteByte(255);
    WriteUInt64(stream, 0);
    WriteSingle(stream, 0);
    stream.WriteByte(0);
    return stream.ToArray();
  }

  private static byte[] CreateStatusPayload(int sectionCount)
  {
    using var stream = new MemoryStream();
    WriteInt32(stream, sectionCount);
    stream.WriteByte(0); // NetworkText literal
    WriteString(stream, "Synchronizing");
    stream.WriteByte(0);
    return stream.ToArray();
  }

  private byte[] CreateTileSectionPayload(int startX, int startY, int width, int height)
  {
    using var body = new MemoryStream();
    WriteInt32(body, startX);
    WriteInt32(body, startY);
    WriteInt16(body, checked((short)width));
    WriteInt16(body, checked((short)height));

    WriteTiles(body, startX, startY, width, height);
    WriteInt16(body, 0); // zero chest count
    WriteInt16(body, 0); // zero sign count
    WriteInt16(body, 0); // zero tile-entity count

    using var compressed = new MemoryStream();
    using (var deflate = new DeflateStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
    {
      body.Position = 0;
      body.CopyTo(deflate);
    }

    return compressed.ToArray();
  }

  private void WriteTiles(Stream body, int startX, int startY, int width, int height)
  {
    WorldTileState[] tiles = new WorldTileState[checked(width * height)];
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int worldX = startX + x;
        int worldY = startY + y;
        tiles[y * width + x] = _options.WorldTiles is null
            ? WorldTileState.Empty
            : _options.WorldTiles[worldY * _options.WorldWidth + worldX];
      }
    }

    int index = 0;
    while (index < tiles.Length)
    {
      WorldTileState tile = tiles[index];
      int repeat = 0;
      while (index + repeat + 1 < tiles.Length && repeat < short.MaxValue &&
          tiles[index + repeat + 1] == tile)
      {
        repeat++;
      }

      WriteTile(body, tile, repeat);
      index += repeat + 1;
    }
  }

  private void WriteTile(Stream stream, WorldTileState tile, int repeat)
  {
    byte flags1 = 0;
    byte flags2 = 0;
    byte flags3 = 0;
    byte flags4 = 0;
    if (tile.Active) flags1 |= 2;
    if (tile.Wall != 0) flags1 |= 4;
    if (tile.HasLiquid)
    {
      flags1 |= tile.Liquid switch
      {
        LiquidContactFlags.Water => (byte)8,
        LiquidContactFlags.Lava => (byte)16,
        LiquidContactFlags.Honey => (byte)24,
        LiquidContactFlags.Shimmer => (byte)8,
        _ => (byte)0
      };
    }

    bool frameImportant = tile.Active && _options.FrameImportant is not null &&
        tile.Type < _options.FrameImportant.Count && _options.FrameImportant[tile.Type];
    if (tile.Active && tile.Type > byte.MaxValue) flags1 |= 0x20;
    if (tile.Wall > byte.MaxValue) flags3 |= 0x40;
    if (repeat > byte.MaxValue) flags1 |= 0x80;
    else if (repeat > 0) flags1 |= 0x40;
    if (flags3 != 0) flags2 |= 1;
    if (flags2 != 0) flags1 |= 1;
    stream.WriteByte(flags1);
    if ((flags1 & 1) != 0) stream.WriteByte(flags2);
    if ((flags2 & 1) != 0) stream.WriteByte(flags3);
    if ((flags3 & 1) != 0) stream.WriteByte(flags4);
    if (tile.Active)
    {
      stream.WriteByte((byte)tile.Type);
      if (tile.Type > byte.MaxValue) stream.WriteByte((byte)(tile.Type >> 8));
      if (frameImportant)
      {
        WriteInt16(stream, tile.FrameX);
        WriteInt16(stream, tile.FrameY);
      }
    }
    if (tile.Wall != 0)
    {
      stream.WriteByte((byte)tile.Wall);
      if (tile.Wall > byte.MaxValue) stream.WriteByte((byte)(tile.Wall >> 8));
    }
    if (tile.HasLiquid) stream.WriteByte(tile.LiquidAmount);
    if (repeat > byte.MaxValue) WriteInt16(stream, checked((short)repeat));
    else if (repeat > 0) stream.WriteByte((byte)repeat);
  }

  private static OutboundPacketIntent CreateIntent(
      byte messageId,
      byte[] payload,
      CommunicationSessionComponent session)
  {
    return new OutboundPacketIntent(
        [session.Epoch],
        null,
        messageId,
        payload,
        CommitVersion.Initial);
  }

  private sealed record PreparedTileSection(int SectionX, int SectionY, byte[] Payload);

  private static string ReadString(ReadOnlySpan<byte> payload)
  {
    int offset = 0;
    uint length = ReadVarUInt(payload, ref offset);
    if (length > int.MaxValue || offset > payload.Length - (int)length)
    {
      throw new ArgumentException("The Terraria string is truncated.", nameof(payload));
    }

    string value = Encoding.UTF8.GetString(payload.Slice(offset, (int)length));
    if (offset + (int)length != payload.Length)
    {
      throw new ArgumentException("The Terraria string contains trailing bytes.", nameof(payload));
    }

    return value;
  }

  private static uint ReadVarUInt(ReadOnlySpan<byte> payload, ref int offset)
  {
    uint value = 0;
    for (int shift = 0; shift <= 28; shift += 7)
    {
      if (offset >= payload.Length) throw new ArgumentException("The Terraria string prefix is truncated.", nameof(payload));
      byte current = payload[offset++];
      if (shift == 28 && current > 7) throw new ArgumentException("The Terraria string prefix is invalid.", nameof(payload));
      value |= (uint)(current & 0x7F) << shift;
      if ((current & 0x80) == 0) return value;
    }

    throw new ArgumentException("The Terraria string prefix is too long.", nameof(payload));
  }

  private static void WriteString(Stream stream, string value)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(value);
    uint remaining = checked((uint)bytes.Length);
    while (remaining >= 0x80)
    {
      stream.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    stream.WriteByte((byte)remaining);
    stream.Write(bytes);
  }

  private static void WriteInt16(Stream stream, short value)
  {
    Span<byte> buffer = stackalloc byte[2];
    BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteUInt16(Stream stream, ushort value)
  {
    Span<byte> buffer = stackalloc byte[2];
    BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteInt32(Stream stream, int value)
  {
    Span<byte> buffer = stackalloc byte[4];
    BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteUInt64(Stream stream, ulong value)
  {
    Span<byte> buffer = stackalloc byte[8];
    BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteSingle(Stream stream, float value)
  {
    Span<byte> buffer = stackalloc byte[4];
    BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
    stream.Write(buffer);
  }
}
