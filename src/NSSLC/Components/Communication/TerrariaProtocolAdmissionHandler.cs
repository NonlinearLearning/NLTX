using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using Terraria.Diagnostics;

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
  private const byte FinishedConnectingMessageId = 129;

  private readonly ICommunicationPacketHandler _gameplay;
  private readonly TerrariaProtocolOptions _options;

  public TerrariaProtocolAdmissionHandler(
      ICommunicationPacketHandler gameplay,
      TerrariaProtocolOptions? options = null)
  {
    _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
    _options = options ?? new TerrariaProtocolOptions();
    _options.Validate();
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
            HandleSpawn(packet.Frame.Payload.Span, messageId, session),
        CommunicationSessionStage.Active => _gameplay.Handle(packet, session, tick),
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
    int sectionCount = (_options.WorldWidth + 199) / 200;
    var responses = new List<OutboundPacketIntent>(sectionCount + 2)
    {
      // MinimalImplementation repeats packet 7 immediately before section transfer. Terraria
      // uses this second world header as the synchronization baseline for packet 10.
      CreateIntent(WorldDataMessageId, CreateWorldDataPayload(), session),
      CreateIntent(StatusTextSizeMessageId, CreateStatusPayload(sectionCount), session)
    };
    for (int startX = 0; startX < _options.WorldWidth; startX += 200)
    {
      int width = Math.Min(200, _options.WorldWidth - startX);
      responses.Add(CreateIntent(
          TileSectionMessageId,
          CreateEmptyTileSectionPayload(startX, width, _options.WorldHeight),
          session));
    }

    responses.Add(CreateIntent(InitialSpawnMessageId, Array.Empty<byte>(), session));
    return responses.AsReadOnly();
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
    WriteInt32(stream, 0);
    stream.WriteByte(1); // day time
    stream.WriteByte(0); // moon phase
    WriteInt16(stream, checked((short)_options.WorldWidth));
    WriteInt16(stream, checked((short)_options.WorldHeight));
    WriteInt16(stream, checked((short)_options.SpawnTileX));
    WriteInt16(stream, checked((short)_options.SpawnTileY));
    WriteInt16(stream, checked((short)(_options.WorldHeight / 2)));
    WriteInt16(stream, checked((short)(_options.WorldHeight * 3 / 4)));
    WriteInt32(stream, 1);
    WriteString(stream, _options.WorldName);
    stream.WriteByte(0); // classic world
    stream.Write(new byte[16]);
    WriteUInt64(stream, 0);
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

  private static byte[] CreateEmptyTileSectionPayload(int startX, int width, int height)
  {
    using var body = new MemoryStream();
    WriteInt32(body, startX);
    WriteInt32(body, 0);
    WriteInt16(body, checked((short)width));
    WriteInt16(body, checked((short)height));

    int tileCount = checked(width * height);
    body.WriteByte(0x80); // Long RLE run; the tile is empty and has no optional fields.
    WriteInt16(body, checked((short)(tileCount - 1)));
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
