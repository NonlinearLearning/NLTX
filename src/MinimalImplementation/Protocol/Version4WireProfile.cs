using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Generated;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Protocol;

/// <summary>Direction-aware adapter over the generated Terraria 4 packet codecs.</summary>
public sealed class Version4WireProfile
{
  private const byte Version4MessageCount = 162;
  private readonly Dictionary<(WireDirection Direction, byte MessageId), IPacketBinding> _readBindings = [];
  private readonly Dictionary<(WireDirection Direction, Type PacketType), IPacketBinding> _writeBindings = [];

  public Version4WireProfile(Version4ProtocolFactsInput factsInput)
  {
    ArgumentNullException.ThrowIfNull(factsInput);
    if (!ProtocolInputs.IsInitialized)
    {
      throw new InvalidOperationException(
        "Version4 generated codecs require the complete protocol facts to be initialized first.");
    }

    if (!Version4ProtocolFactsFactory.IsPublished(factsInput)
        || !string.Equals(
          Version4ProtocolFactsFactory.PublishedContentFactsSha256,
          factsInput.ContentFactsSha256,
          StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "The wire profile facts do not match the published protocol inputs.");
    }

    Key = $"{TerrariaV4Protocol.Name}:{TerrariaV4Protocol.Version}:{Version4GeneratedWireFacts.InputFingerprint}:{ComputeProtocolFingerprint()}:{factsInput.ContentFactsSha256}";
    foreach (ProtocolPacketDescriptor descriptor in TerrariaV4Protocol.All)
    {
      RegisterDescriptor(descriptor);
    }

    ValidateMessageIdCoverage(TerrariaV4Protocol.All);
    ValidateDirectionalBindingCoverage(TerrariaV4Protocol.All);
    if (_readBindings.Count == 0 || _writeBindings.Count == 0)
    {
      throw new InvalidOperationException("The generated Version4 packet catalog is empty.");
    }
  }

  public string Key { get; }

  public int DecodeBindingCount => _readBindings.Count;

  public int EncodeBindingCount => _writeBindings.Count;

  public bool CanDecode(WireDirection direction, byte messageId)
  {
    return _readBindings.ContainsKey((direction, messageId));
  }

  private static string ComputeProtocolFingerprint()
  {
    string canonicalDescriptors = string.Join(
      "\n",
      TerrariaV4Protocol.All
        .OrderBy(static descriptor => descriptor.MessageId)
        .ThenBy(static descriptor => descriptor.Name, StringComparer.Ordinal)
        .Select(static descriptor =>
          $"{descriptor.MessageId}|{descriptor.Name}|{descriptor.WireDirection}|{descriptor.Directions}|{descriptor.PacketType.FullName}"));
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalDescriptors)));
  }

  public object Decode(WireDirection direction, byte messageId, ReadOnlyMemory<byte> body)
  {
    if (!IsSingleDirection(direction))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    if (!_readBindings.TryGetValue((direction, messageId), out IPacketBinding? binding))
    {
      throw new Version4PacketFormatException(
        messageId,
        null,
        0,
        $"No Version4 decoder is registered for message {messageId} in direction {direction}.");
    }

    object packet = binding.Decode(body);
    if (packet is NetModulesPacket modulePacket)
    {
      ushort logicalModuleId = Version4SteamModuleIdMap.ToLogicalModuleId(modulePacket.ModuleId);
      if (logicalModuleId == ushort.MaxValue ||
          !ModulePayloadMatchesDirection(direction, modulePacket, logicalModuleId))
      {
        throw new Version4PacketFormatException(
          messageId,
          PacketReadErrorCode.InvalidCodecData,
          0,
          $"Packet 82 module {modulePacket.ModuleId} payload is invalid for direction {direction}.");
      }
    }

    return packet;
  }

  public Version4EncodedPacket Encode(
    WireDirection direction,
    object packet,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!IsSingleDirection(direction))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    if (packet is DevCommandsPacket
        && direction == WireDirection.ServerToClient
        && !enableDebugCommands)
    {
      throw new InvalidOperationException(
        "Packet 94 encoding requires the explicit Version4 debug-command option.");
    }

    object packetToEncode = packet;
    if (packet is NetModulesPacket modulePacket)
    {
      if (!ModulePayloadMatchesDirection(direction, modulePacket, modulePacket.ModuleId))
      {
        throw new InvalidOperationException(
          $"Packet 82 module {modulePacket.ModuleId} payload does not match direction {direction}.");
      }

      packetToEncode = new NetModulesPacket
      {
        ModuleId = Version4SteamModuleIdMap.ToWireModuleId(modulePacket.ModuleId),
        Data = modulePacket.Data
      };
    }

    if (!_writeBindings.TryGetValue((direction, packetToEncode.GetType()), out IPacketBinding? binding))
    {
      throw new InvalidOperationException(
        $"No Version4 encoder is registered for {packetToEncode.GetType().Name} in direction {direction}.");
    }

    return new Version4EncodedPacket(binding.MessageId, binding.EncodePayload(packetToEncode));
  }

  public OutboundPacketIntent CreateOutboundIntent(
    object packet,
    IReadOnlyCollection<SessionEpoch> targetEpochs,
    SessionEpoch? ignoreEpoch,
    CommitVersion commitVersion,
    bool enableDebugCommands = false)
  {
    ArgumentNullException.ThrowIfNull(targetEpochs);
    Version4EncodedPacket encoded = Encode(
      WireDirection.ServerToClient,
      packet,
      enableDebugCommands);
    return new OutboundPacketIntent(
      targetEpochs,
      ignoreEpoch,
      encoded.MessageId,
      encoded.Payload,
      commitVersion);
  }

  public InboundVersion4Packet DecodeInbound(
    InboundRawFrame frame,
    InboundPacketEnvelope envelope,
    bool enableDebugCommands)
  {
    ArgumentNullException.ThrowIfNull(frame);
    ArgumentNullException.ThrowIfNull(envelope);
    if (envelope.Epoch != frame.Epoch
        || envelope.Sequence != frame.Sequence
        || envelope.ReceivedAt != frame.ReceivedAt
        || envelope.MessageId != frame.Frame.MessageId
        || envelope.ModuleId != frame.Frame.ModuleId
        || !envelope.RawFrame.Span.SequenceEqual(frame.Frame.FrameBytes.Span))
    {
      throw new InvalidOperationException(
        "The P02 envelope factory changed transport-owned frame identity or bytes.");
    }

    if (frame.Frame.MessageId >= Version4MessageCount)
    {
      if (!string.Equals(envelope.Stage, "Active", StringComparison.Ordinal))
      {
        return CreateInboundResult(
          envelope,
          Version4PacketHandling.RejectStage,
          null,
          "Unknown message ids are consumed only after the session reaches the active stage.");
      }

      return CreateInboundResult(
        envelope,
        Version4PacketHandling.ConsumedUnknownPacket,
        null,
        "The complete bounded frame has an unregistered Version4 message id.");
    }

    Version4PacketPolicyResult reservedPolicy = Version4ReservedPacketPolicy.Evaluate(
      frame.Frame.MessageId,
      envelope.Stage,
      enableDebugCommands);
    if (reservedPolicy.Handling == Version4PacketHandling.RejectStage)
    {
      return CreateInboundResult(
        envelope,
        reservedPolicy.Handling,
        null,
        reservedPolicy.Reason);
    }

    if (frame.Frame.MessageId == 94)
    {
      object debugPacket = Decode(
        WireDirection.ClientToServer,
        frame.Frame.MessageId,
        frame.Frame.Payload);
      return CreateInboundResult(
        envelope,
        reservedPolicy.Handling,
        enableDebugCommands ? debugPacket : null,
        reservedPolicy.Reason);
    }

    if (reservedPolicy.Handling == Version4PacketHandling.ConsumedReserved)
    {
      return CreateInboundResult(
        envelope,
        reservedPolicy.Handling,
        null,
        reservedPolicy.Reason);
    }

    if (frame.Frame.MessageId == PacketFrameCodec.NetModulesMessageId
        && IsServerOnlyModulePayload(frame.Frame))
    {
      return CreateInboundResult(
        envelope,
        Version4PacketHandling.RejectDirection,
        null,
        $"Packet 82 module {frame.Frame.ModuleId} payload is authored by the server.");
    }

    if (frame.Frame.MessageId == PacketFrameCodec.NetModulesMessageId
        && frame.Frame.ModuleId is ushort moduleId
        && !ProtocolInputs.Instance.ModuleCodecs.ContainsKey(moduleId))
    {
      if (!string.Equals(envelope.Stage, "Active", StringComparison.Ordinal))
      {
        return CreateInboundResult(
          envelope,
          Version4PacketHandling.RejectStage,
          null,
          "Unknown Packet 82 modules are consumed only after the active stage.");
      }

      return CreateInboundResult(
        envelope,
        Version4PacketHandling.ConsumedUnknownModule,
        null,
        $"Packet 82 module {moduleId} is unknown and was consumed at its bounded frame boundary.");
    }

    if (!CanDecode(WireDirection.ClientToServer, frame.Frame.MessageId))
    {
      return CreateInboundResult(
        envelope,
        Version4PacketHandling.RejectDirection,
        null,
        $"Message {frame.Frame.MessageId} has no client-to-server wire layout.");
    }

    object packet = Decode(
      WireDirection.ClientToServer,
      frame.Frame.MessageId,
      frame.Frame.Payload);
    if (frame.Frame.MessageId == PacketFrameCodec.NetModulesMessageId
        && (packet is not NetModulesPacket modulePacket
          || frame.Frame.ModuleId != modulePacket.ModuleId))
    {
      throw new Version4PacketFormatException(
        frame.Frame.MessageId,
        PacketReadErrorCode.InvalidCodecData,
        0,
        "The framed Packet 82 module id does not match its decoded payload.");
    }

    return CreateInboundResult(
      envelope,
      Version4PacketHandling.Typed,
      packet,
      "A bounded, direction-registered packet was decoded.");
  }

  private static InboundVersion4Packet CreateInboundResult(
    InboundPacketEnvelope envelope,
    Version4PacketHandling handling,
    object? packet,
    string reason)
  {
    return new InboundVersion4Packet(envelope, handling, packet, reason);
  }

  private static void ValidateMessageIdCoverage(
    IReadOnlyList<ProtocolPacketDescriptor> descriptors)
  {
    var messageIds = new HashSet<byte>();
    foreach (ProtocolPacketDescriptor descriptor in descriptors)
    {
      if (descriptor.MessageId >= Version4MessageCount)
      {
        throw new InvalidOperationException(
          $"Version4 packet {descriptor.Name} uses out-of-range message id {descriptor.MessageId}.");
      }

      messageIds.Add(descriptor.MessageId);
    }

    if (messageIds.Count != Version4MessageCount)
    {
      throw new InvalidOperationException(
        $"Version4 wire catalog covers {messageIds.Count} unique message ids; expected {Version4MessageCount}.");
    }

    for (byte messageId = 0; messageId < Version4MessageCount; messageId++)
    {
      if (!messageIds.Contains(messageId))
      {
        throw new InvalidOperationException($"Version4 wire catalog is missing message id {messageId}.");
      }
    }
  }

  private void ValidateDirectionalBindingCoverage(
    IReadOnlyList<ProtocolPacketDescriptor> descriptors)
  {
    int expectedReadBindings = 0;
    int expectedWriteBindings = 0;
    foreach (ProtocolPacketDescriptor descriptor in descriptors)
    {
      if (descriptor.WireDirection == WireDirection.None ||
          (descriptor.WireDirection & ~WireDirection.Bidirectional) != 0)
      {
        throw new InvalidOperationException(
          $"Version4 packet {descriptor.Name} has an invalid wire direction " +
          $"{descriptor.WireDirection}.");
      }

      if (descriptor.Directions == PacketCodecDirections.None ||
          (descriptor.Directions & ~PacketCodecDirections.Bidirectional) != 0)
      {
        throw new InvalidOperationException(
          $"Version4 packet {descriptor.Name} has invalid codec directions " +
          $"{descriptor.Directions}.");
      }

      foreach (WireDirection direction in GetDirections(descriptor.WireDirection))
      {
        if ((descriptor.Directions & PacketCodecDirections.Decode) == 0)
        {
          throw new InvalidOperationException(
            $"Version4 packet {descriptor.Name} has no decoder for direction {direction}.");
        }

        if ((descriptor.Directions & PacketCodecDirections.Encode) == 0)
        {
          throw new InvalidOperationException(
            $"Version4 packet {descriptor.Name} has no encoder for direction {direction}.");
        }

        expectedReadBindings++;
        expectedWriteBindings++;
      }
    }

    if (_readBindings.Count != expectedReadBindings ||
        _writeBindings.Count != expectedWriteBindings)
    {
      throw new InvalidOperationException(
        $"Version4 directional binding coverage is incomplete: expected " +
        $"{expectedReadBindings} readers/{expectedWriteBindings} writers, found " +
        $"{_readBindings.Count} readers/{_writeBindings.Count} writers.");
    }
  }

  private static bool IsSingleDirection(WireDirection direction)
  {
    return direction is WireDirection.ClientToServer or WireDirection.ServerToClient;
  }

  private static bool ModulePayloadMatchesDirection(
    WireDirection direction,
    NetModulesPacket packet,
    ushort logicalModuleId)
  {
    return logicalModuleId switch
    {
      1 => direction == WireDirection.ClientToServer
        ? packet.Data is Version4ModulePayloads.TextCommandData
        : packet.Data is Packet82TextData,
      3 or 4 or 9 => direction == WireDirection.ServerToClient,
      7 => packet.Data is Packet82TeleportPylonData pylon
        && (direction == WireDirection.ClientToServer ? pylon.Action == 2 : pylon.Action is 0 or 1),
      10 => direction == WireDirection.ClientToServer
        ? packet.Data is Packet82BannerClaimRequestData
        : packet.Data is Packet82BannerClaimResponseData
          || packet.Data is Packet82BannerData { Action: 0 or 1 or 2 },
      11 => direction == WireDirection.ClientToServer
        ? packet.Data is Version4ModulePayloads.CraftingRequestData
        : packet.Data is Version4ModulePayloads.CraftingApprovalData,
      14 => direction == WireDirection.ServerToClient
        && packet.Data is Version4ModulePayloads.UnbreakableWallNotificationData,
      _ => true
    };
  }

  private static bool IsServerOnlyModulePayload(RawPacketFrame frame)
  {
    if (frame.ModuleId is not ushort wireModuleId)
    {
      return false;
    }

    ushort logicalModuleId = Version4SteamModuleIdMap.ToLogicalModuleId(wireModuleId);
    if (logicalModuleId == ushort.MaxValue || logicalModuleId is 3 or 4 or 9 or 14)
    {
      return true;
    }

    // The module id occupies the first two payload bytes. Only inspect the explicit action;
    // truncated and unknown actions still go through the bounded codec's format checks.
    ReadOnlySpan<byte> payload = frame.Payload.Span;
    if (payload.Length <= sizeof(ushort))
    {
      return false;
    }

    byte action = payload[sizeof(ushort)];
    return logicalModuleId switch
    {
      7 => action is 0 or 1,
      10 => action is 0 or 1 or 2 or 4,
      // A crafting approval is the one-byte server response. A client request always
      // carries at least the material and chest collection count prefixes.
      11 => payload.Length == sizeof(ushort) + sizeof(bool),
      _ => false
    };
  }

  private void RegisterDescriptor(ProtocolPacketDescriptor descriptor)
  {
    Type packetApi = typeof(TerrariaV4Protocol.Packets).GetNestedType(
      descriptor.Name,
      BindingFlags.Public)
      ?? throw new InvalidOperationException(
        $"Generated packet API {descriptor.Name} is missing from TerrariaV4Protocol.");
    IPacketBinding binding = CreateBinding(descriptor, packetApi);
    foreach (WireDirection direction in GetDirections(descriptor.WireDirection))
    {
      if ((descriptor.Directions & PacketCodecDirections.Decode) != 0
          && !_readBindings.TryAdd((direction, descriptor.MessageId), binding))
      {
        throw new InvalidOperationException(
          $"Duplicate Version4 decoder for message {descriptor.MessageId} in direction {direction}.");
      }

      if ((descriptor.Directions & PacketCodecDirections.Encode) != 0
          && !_writeBindings.TryAdd((direction, descriptor.PacketType), binding))
      {
        throw new InvalidOperationException(
          $"Duplicate Version4 encoder for {descriptor.PacketType.Name} in direction {direction}.");
      }
    }
  }

  private static IEnumerable<WireDirection> GetDirections(WireDirection directions)
  {
    if ((directions & WireDirection.ClientToServer) != 0)
    {
      yield return WireDirection.ClientToServer;
    }

    if ((directions & WireDirection.ServerToClient) != 0)
    {
      yield return WireDirection.ServerToClient;
    }
  }

  private static IPacketBinding CreateBinding(
    ProtocolPacketDescriptor descriptor,
    Type packetApi)
  {
    MethodInfo createReader = packetApi.GetMethod(
      "CreateReader",
      [typeof(ReadOnlyMemory<byte>)])
      ?? throw MissingApi(packetApi, "CreateReader(ReadOnlyMemory<byte>)");
    Type readerType = createReader.ReturnType;
    MethodInfo readFrameDetailed = readerType.GetMethod("ReadFrameDetailed", Type.EmptyTypes)
      ?? throw MissingApi(readerType, "ReadFrameDetailed()");
    MethodInfo createWriter = packetApi.GetMethod("CreateWriter", Type.EmptyTypes)
      ?? throw MissingApi(packetApi, "CreateWriter()");
    Type writerType = createWriter.ReturnType;
    MethodInfo tryWrite = writerType.GetMethod("TryWrite", [descriptor.PacketType])
      ?? throw MissingApi(writerType, $"TryWrite({descriptor.PacketType.Name})");
    MethodInfo factory = typeof(Version4WireProfile).GetMethod(
      nameof(CreateTypedBinding),
      BindingFlags.NonPublic | BindingFlags.Static)
      ?? throw new InvalidOperationException("The typed packet binding factory is missing.");

    try
    {
      return (IPacketBinding)factory
        .MakeGenericMethod(descriptor.PacketType, readerType, writerType)
        .Invoke(null, [descriptor.MessageId, createReader, readFrameDetailed, createWriter, tryWrite])!;
    }
    catch (TargetInvocationException exception) when (exception.InnerException is not null)
    {
      throw new InvalidOperationException(
        $"Generated Version4 codec binding for {descriptor.Name} is invalid.",
        exception.InnerException);
    }
  }

  private static IPacketBinding CreateTypedBinding<TPacket, TReader, TWriter>(
    byte messageId,
    MethodInfo createReader,
    MethodInfo readFrameDetailed,
    MethodInfo createWriter,
    MethodInfo tryWrite)
  {
    var readerFactory = (Func<ReadOnlyMemory<byte>, TReader>)createReader.CreateDelegate(
      typeof(Func<ReadOnlyMemory<byte>, TReader>));
    var reader = (Func<TReader, PacketReadResult<TPacket>>)readFrameDetailed.CreateDelegate(
      typeof(Func<TReader, PacketReadResult<TPacket>>));
    var writerFactory = (Func<TWriter>)createWriter.CreateDelegate(typeof(Func<TWriter>));
    var writer = (Func<TWriter, TPacket, MemoryStream?>)tryWrite.CreateDelegate(
      typeof(Func<TWriter, TPacket, MemoryStream?>));
    return new PacketBinding<TPacket, TReader, TWriter>(
      messageId,
      readerFactory,
      reader,
      writerFactory,
      writer);
  }

  private static InvalidOperationException MissingApi(Type type, string method)
  {
    return new InvalidOperationException($"Generated packet API {type.FullName}.{method} is missing.");
  }

  private interface IPacketBinding
  {
    byte MessageId { get; }

    object Decode(ReadOnlyMemory<byte> body);

    byte[] EncodePayload(object packet);
  }

  private sealed class PacketBinding<TPacket, TReader, TWriter> : IPacketBinding
  {
    private readonly Func<ReadOnlyMemory<byte>, TReader> _readerFactory;
    private readonly Func<TReader, PacketReadResult<TPacket>> _reader;
    private readonly Func<TWriter> _writerFactory;
    private readonly Func<TWriter, TPacket, MemoryStream?> _writer;

    public PacketBinding(
      byte messageId,
      Func<ReadOnlyMemory<byte>, TReader> readerFactory,
      Func<TReader, PacketReadResult<TPacket>> reader,
      Func<TWriter> writerFactory,
      Func<TWriter, TPacket, MemoryStream?> writer)
    {
      MessageId = messageId;
      _readerFactory = readerFactory;
      _reader = reader;
      _writerFactory = writerFactory;
      _writer = writer;
    }

    public byte MessageId { get; }

    public object Decode(ReadOnlyMemory<byte> body)
    {
      PacketReadResult<TPacket> result = _reader(_readerFactory(body));
      if (!result.Success)
      {
        PacketReadError error = result.Error!;
        throw new Version4PacketFormatException(
          MessageId,
          error.Code,
          error.Offset,
          error.Message);
      }

      if (result.Consumed != body.Length)
      {
        throw new Version4PacketFormatException(
          MessageId,
          PacketReadErrorCode.TrailingBytes,
          result.Consumed,
          "The Version4 packet body contains trailing bytes.");
      }

      return result.Packet!;
    }

    public byte[] EncodePayload(object packet)
    {
      if (packet.GetType() != typeof(TPacket))
      {
        throw new InvalidOperationException("Packet type does not match the Version4 binding.");
      }

      using MemoryStream? body = _writer(_writerFactory(), (TPacket)packet);
      if (body is null || body.Length > PacketFrameCodec.MaximumPayloadBytes)
      {
        throw new InvalidOperationException(
          $"Version4 packet {MessageId} was rejected or exceeds the frame body limit.");
      }

      return body.ToArray();
    }
  }
}
