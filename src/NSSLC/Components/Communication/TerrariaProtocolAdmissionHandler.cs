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
  private const int SectionWidth = 200;
  private const int SectionHeight = 150;
  private const int TilePixelSize = 16;

  private readonly ICommunicationPacketHandler _gameplay;
  private readonly TerrariaProtocolOptions _options;
  private readonly IReadOnlyList<PreparedTileSection> _initialSections;
  private readonly IReadOnlyDictionary<(int X, int Y), PreparedTileSection> _sections;
  private readonly IReadOnlyList<bool> _frameImportant;

  public TerrariaProtocolAdmissionHandler(
      ICommunicationPacketHandler gameplay,
      TerrariaProtocolOptions? options = null)
  {
    _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
    _options = options ?? new TerrariaProtocolOptions();
    _options.Validate();
    _frameImportant = CreateFrameImportance(_options.FrameImportant);
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
        CommunicationSessionStage.Active => HandleActivePacket(packet, session, tick),
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
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(sectionCount), session)
    };
    foreach (PreparedTileSection section in _initialSections)
    {
      session.MarkWorldSectionSent(section.SectionX, section.SectionY);
      responses.Add(CreateIntent(TileSectionMessageId, section.Payload, session));
    }

    responses.Add(CreateIntent(InitialSpawnMessageId, Array.Empty<byte>(), session));
    return responses.AsReadOnly();
  }

  private IReadOnlyDictionary<(int X, int Y), PreparedTileSection> PrepareInitialSections(
      out IReadOnlyList<PreparedTileSection> initialSections)
  {
    long started = Stopwatch.GetTimestamp();
    var sections = new List<PreparedTileSection>(
        ((_options.WorldWidth + SectionWidth - 1) / SectionWidth) *
        ((_options.WorldHeight + SectionHeight - 1) / SectionHeight));
    long totalBytes = 0;
    for (int startY = 0; startY < _options.WorldHeight; startY += SectionHeight)
    {
      int height = Math.Min(SectionHeight, _options.WorldHeight - startY);
      for (int startX = 0; startX < _options.WorldWidth; startX += SectionWidth)
      {
        int width = Math.Min(SectionWidth, _options.WorldWidth - startX);
        byte[] payload = CreateTileSectionPayload(startX, startY, width, height);
        sections.Add(new PreparedTileSection(startX / SectionWidth, startY / SectionHeight, payload));
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
    int centerX = spawnTileX / SectionWidth;
    int centerY = spawnTileY / SectionHeight;
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

  private IReadOnlyList<OutboundPacketIntent> HandleActivePacket(
      InboundCommunicationPacket packet,
      CommunicationSessionComponent session,
      long tick)
  {
    if (packet.Envelope.MessageId == RequestSectionMessageId)
    {
      return HandleSectionRequest(packet.Frame.Payload.Span, session);
    }

    IReadOnlyList<OutboundPacketIntent> gameplayIntents = _gameplay.Handle(packet, session, tick);
    if (packet.Envelope.MessageId != GameCommunicationAdapter.PlayerControlsMessageId ||
        !TryReadPlayerPosition(packet.Frame.Payload.Span, out float positionX, out float positionY))
    {
      return gameplayIntents;
    }

    IReadOnlyList<OutboundPacketIntent> sectionIntents =
        CreateMovementSectionIntents(positionX, positionY, session);
    if (sectionIntents.Count == 0)
    {
      return gameplayIntents;
    }

    var combined = new List<OutboundPacketIntent>(gameplayIntents.Count + sectionIntents.Count);
    combined.AddRange(gameplayIntents);
    combined.AddRange(sectionIntents);
    return combined.AsReadOnly();
  }

  private IReadOnlyList<OutboundPacketIntent> CreateMovementSectionIntents(
      float positionX,
      float positionY,
      CommunicationSessionComponent session)
  {
    int tileX = ToTileCoordinate(positionX, _options.WorldWidth);
    int tileY = ToTileCoordinate(positionY, _options.WorldHeight);
    int centerX = tileX / SectionWidth;
    int centerY = tileY / SectionHeight;
    var sections = new List<PreparedTileSection>(9);
    for (int sectionY = centerY - 1; sectionY <= centerY + 1; sectionY++)
    {
      for (int sectionX = centerX - 1; sectionX <= centerX + 1; sectionX++)
      {
        if (!_sections.TryGetValue((sectionX, sectionY), out PreparedTileSection? section) ||
            session.HasSentWorldSection(sectionX, sectionY))
        {
          continue;
        }

        sections.Add(section);
      }
    }

    if (sections.Count == 0)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info(
        "ecs.protocol.movement_sections",
        $"epoch={session.Epoch.Value} tile={tileX},{tileY} " +
        $"center={centerX},{centerY} count={sections.Count}");
    var responses = new List<OutboundPacketIntent>(sections.Count + 1)
    {
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(sections.Count), session)
    };
    foreach (PreparedTileSection section in sections)
    {
      session.MarkWorldSectionSent(section.SectionX, section.SectionY);
      responses.Add(CreateIntent(TileSectionMessageId, section.Payload, session));
    }

    return responses.AsReadOnly();
  }

  private static int ToTileCoordinate(float position, int worldSize)
  {
    if (!float.IsFinite(position))
    {
      return 0;
    }

    float tilePosition = position / TilePixelSize;
    if (tilePosition <= 0f)
    {
      return 0;
    }

    if (tilePosition >= worldSize - 1)
    {
      return worldSize - 1;
    }

    return (int)tilePosition;
  }

  private static bool TryReadPlayerPosition(
      ReadOnlySpan<byte> payload,
      out float positionX,
      out float positionY)
  {
    positionX = 0;
    positionY = 0;
    const int positionOffset = 6;
    if (payload.Length < positionOffset + sizeof(float) * 2)
    {
      return false;
    }

    positionX = BitConverter.Int32BitsToSingle(
        BinaryPrimitives.ReadInt32LittleEndian(payload[positionOffset..]));
    positionY = BitConverter.Int32BitsToSingle(
        BinaryPrimitives.ReadInt32LittleEndian(payload[(positionOffset + sizeof(float))..]));
    return float.IsFinite(positionX) && float.IsFinite(positionY);
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
    if (!_sections.TryGetValue((sectionX, sectionY), out PreparedTileSection? section))
    {
      ServerLog.Warning(
          "ecs.protocol.section_rejected",
          $"epoch={session.Epoch.Value} section={sectionX},{sectionY} reason=out_of_bounds");
      return Array.Empty<OutboundPacketIntent>();
    }

    ServerLog.Info(
        "ecs.protocol.section_request",
        $"epoch={session.Epoch.Value} section={sectionX},{sectionY}");
    session.MarkWorldSectionSent(sectionX, sectionY);
    return [
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(1), session),
      CreateIntent(TileSectionMessageId, section.Payload, session)
    ];
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
    WriteInt16(stream, 0);
    WriteInt16(stream, 0);
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
    // Packet 10 is serialized in y-major order. This is independent from the column-major
    // order used by the .wld tile payload codec.
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int worldX = startX + x;
        int worldY = startY + y;
        WorldTileState tile = ReadWorldTile(worldX, worldY);
        int repeat = 0;

        WriteTile(body, tile, repeat);
      }
    }
  }

  private WorldTileState ReadWorldTile(int worldX, int worldY)
  {
    if (_options.WorldTiles is null)
    {
      return WorldTileState.Empty;
    }

    WorldTileState source = _options.WorldTiles[worldY * _options.WorldWidth + worldX];
    return source;
  }

  private void WriteTile(Stream stream, WorldTileState tile, int repeat)
  {
    byte flags1 = 0;
    byte flags2 = 0;
    byte flags3 = 0;
    byte flags4 = 0;
    bool hasTileColor = tile.Active && tile.TileColor != 0;
    bool hasWallColor = tile.Wall != 0 && tile.WallColor != 0;
    bool hasLiquid = tile.LiquidAmount != 0;
    bool hasHighTileType = tile.Active && tile.Type > byte.MaxValue;
    bool hasHighWall = tile.Wall > byte.MaxValue;
    if (tile.Active)
    {
      flags1 |= 0x02;
    }

    if (tile.Wall != 0)
    {
      flags1 |= 0x04;
    }

    if (hasHighTileType)
    {
      flags1 |= 0x20;
    }

    if (hasLiquid)
    {
      flags1 |= tile.Liquid switch
      {
        LiquidContactFlags.Water => (byte)8,
        LiquidContactFlags.Lava => (byte)16,
        LiquidContactFlags.Honey => (byte)24,
        LiquidContactFlags.Shimmer => (byte)8,
        _ => throw new InvalidDataException("The world contains an unknown liquid type.")
      };
    }

    if ((tile.Wires & 0x01) != 0)
    {
      flags2 |= 0x02;
    }

    if ((tile.Wires & 0x02) != 0)
    {
      flags2 |= 0x04;
    }

    if ((tile.Wires & 0x04) != 0)
    {
      flags2 |= 0x08;
    }

    int slopeCode = tile.HalfBlock ? 1 : tile.Slope == 0 ? 0 : tile.Slope + 1;
    if (slopeCode > 8)
    {
      throw new InvalidDataException("The world contains an invalid tile slope.");
    }

    flags2 |= (byte)(slopeCode << 4);
    if (tile.Actuator)
    {
      flags3 |= 0x02;
    }

    if (tile.Inactive)
    {
      flags3 |= 0x04;
    }

    if (hasTileColor)
    {
      flags3 |= 0x08;
    }

    if (hasWallColor)
    {
      flags3 |= 0x10;
    }

    if ((tile.Wires & 0x08) != 0)
    {
      flags3 |= 0x20;
    }

    if (hasHighWall)
    {
      flags3 |= 0x40;
    }

    if (hasLiquid && tile.Liquid == LiquidContactFlags.Shimmer)
    {
      flags3 |= 0x80;
    }

    if ((tile.TileHeader3 & 0x20) != 0)
    {
      flags4 |= 0x02;
    }

    if ((tile.TileHeader3 & 0x40) != 0)
    {
      flags4 |= 0x04;
    }

    if ((tile.TileHeader3 & 0x80) != 0)
    {
      flags4 |= 0x08;
    }

    if ((tile.WallHeader & 0x80) != 0)
    {
      flags4 |= 0x10;
    }

    if (flags4 != 0)
    {
      flags3 |= 0x01;
    }

    if (flags3 != 0)
    {
      flags2 |= 0x01;
    }

    if (flags2 != 0)
    {
      flags1 |= 0x01;
    }

    bool frameImportant = tile.Active && tile.Type < _frameImportant.Count &&
        _frameImportant[tile.Type];
    if (repeat > byte.MaxValue)
    {
      flags1 |= 0x80;
    }
    else if (repeat > 0)
    {
      flags1 |= 0x40;
    }

    stream.WriteByte(flags1);
    if ((flags1 & 1) != 0) stream.WriteByte(flags2);
    if ((flags2 & 1) != 0) stream.WriteByte(flags3);
    if ((flags3 & 1) != 0) stream.WriteByte(flags4);
    if (tile.Active)
    {
      stream.WriteByte((byte)tile.Type);
      if (hasHighTileType) stream.WriteByte((byte)(tile.Type >> 8));
      if (frameImportant)
      {
        WriteInt16(stream, tile.FrameX);
        WriteInt16(stream, tile.FrameY);
      }

      if (hasTileColor)
      {
        stream.WriteByte(tile.TileColor);
      }
    }

    if (tile.Wall != 0)
    {
      stream.WriteByte((byte)tile.Wall);
      if (hasWallColor)
      {
        stream.WriteByte(tile.WallColor);
      }
    }

    if (hasLiquid) stream.WriteByte(tile.LiquidAmount);
    if (hasHighWall) stream.WriteByte((byte)(tile.Wall >> 8));
    if (repeat > byte.MaxValue) WriteInt16(stream, checked((short)repeat));
    else if (repeat > 0) stream.WriteByte((byte)repeat);
  }

  private static IReadOnlyList<bool> CreateFrameImportance(IReadOnlyList<bool>? source)
  {
    return source is null ? new bool[753] : Array.AsReadOnly(source.ToArray());
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
