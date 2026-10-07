using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace NSSLC.Infrastructure.Network;

/// <summary>Steam 1.4.5.8 wire layouts and directions for world-item messages.</summary>
public sealed class SteamItemPacketCodec
{
  private const byte KnownFlags = 0x0F;
  private const int MaximumItemBytes = 30;
  private const int MaximumItemSlot = 400;
  private static readonly byte[] ReplacedMessageIds = [
    21, 22, 39, 88, 90, 145, 148, 151, 160
  ];

  public IReadOnlyList<PacketBinding> CreateBindings()
  {
    PacketBinding[] bindings = [
      CreateSyncItemBinding(PacketDirection.ClientToServer),
      CreateSyncItemBinding(PacketDirection.ServerToClient),
      CreateItemOwnerBinding(),
      CreateReleaseItemOwnershipBinding(PacketDirection.ClientToServer),
      CreateReleaseItemOwnershipBinding(PacketDirection.ServerToClient),
      CreateInstancedItemBinding(PacketDirection.ClientToServer),
      CreateInstancedItemBinding(PacketDirection.ServerToClient)
    ];
    return Array.AsReadOnly(bindings);
  }

  /// <summary>
  /// Replaces generated Version4 world-item bindings with Steam 1.4.5.8 layouts and directions.
  /// Messages 145 and 148 are omitted because Steam folds them into 21/90 flags.
  /// </summary>
  public IReadOnlyList<PacketBinding> ReplaceBindings(
    IEnumerable<PacketBinding> generatedBindings)
  {
    ArgumentNullException.ThrowIfNull(generatedBindings);
    PacketBinding[] generated = generatedBindings.ToArray();
    var bindings = generated
      .Where(static binding => !IsReplacedMessage(binding.MessageId))
      .Concat(CreateBindings())
      .Append(FindGeneratedBinding(generated, 88, PacketDirection.ServerToClient))
      .Append(FindGeneratedBinding(generated, 151, PacketDirection.ClientToServer))
      .Append(FindGeneratedBinding(generated, 151, PacketDirection.ServerToClient))
      .Append(FindGeneratedBinding(generated, 160, PacketDirection.ServerToClient))
      .ToArray();
    return Array.AsReadOnly(bindings);
  }

  public PacketBinding<SyncItemPacket> CreateSyncItemBinding(
    PacketDirection direction)
  {
    if (direction is not (PacketDirection.ClientToServer or
        PacketDirection.ServerToClient))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    return new PacketBinding<SyncItemPacket>(
      21,
      direction,
      DecodeSyncItemBody,
      EncodeSyncItemBodyStream);
  }

  public PacketBinding<ItemOwnerPacket> CreateItemOwnerBinding()
  {
    return new PacketBinding<ItemOwnerPacket>(
      22,
      PacketDirection.ServerToClient,
      DecodeItemOwnerBody,
      EncodeItemOwnerBodyStream);
  }

  public PacketBinding<ReleaseItemOwnershipPacket> CreateReleaseItemOwnershipBinding(
    PacketDirection direction)
  {
    if (direction is not (PacketDirection.ClientToServer or
        PacketDirection.ServerToClient))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    return new PacketBinding<ReleaseItemOwnershipPacket>(
      39,
      direction,
      DecodeReleaseItemOwnershipBody,
      EncodeReleaseItemOwnershipBodyStream);
  }

  public PacketBinding<InstancedItemPacket> CreateInstancedItemBinding(
    PacketDirection direction)
  {
    if (direction is not (PacketDirection.ClientToServer or
        PacketDirection.ServerToClient))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    return new PacketBinding<InstancedItemPacket>(
      90,
      direction,
      DecodeInstancedItemBody,
      EncodeInstancedItemBodyStream);
  }

  public static PacketReadResult<ReleaseItemOwnershipPacket> DecodeReleaseItemOwnershipBody(
    ReadOnlyMemory<byte> body)
  {
    var reader = new PacketWireReader(body);
    try
    {
      short itemIndex = reader.ReadInt16();
      bool forceAssignToServer = reader.ReadBoolean();
      reader.RequireFrameEnd();
      if (itemIndex < 0 || itemIndex >= MaximumItemSlot)
      {
        return Invalid<ReleaseItemOwnershipPacket>(reader, "ItemIndex",
          "Steam item ownership release contains an invalid slot.");
      }

      return PacketReadResult<ReleaseItemOwnershipPacket>.Succeeded(
        new ReleaseItemOwnershipPacket
        {
          ItemIndex = itemIndex,
          ForceAssignToServer = forceAssignToServer
        },
        reader.Position);
    }
    catch (PacketWireTruncationException exception)
    {
      return Failed<ReleaseItemOwnershipPacket>(PacketReadStatus.Truncated,
        PacketReadErrorCode.Truncated, reader.Position, exception.Message);
    }
    catch (PacketWireFormatException exception)
    {
      return Failed<ReleaseItemOwnershipPacket>(PacketReadStatus.InvalidData,
        exception.Code, reader.Position, exception.Message);
    }
  }

  public static byte[] EncodeReleaseItemOwnershipBody(
    ReleaseItemOwnershipPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex < 0 || packet.ItemIndex >= MaximumItemSlot)
    {
      throw new PacketEncodingException(
        "Steam item ownership release contains an invalid slot.");
    }

    using var stream = new MemoryStream(3);
    var writer = new PacketWireWriter(stream, 3);
    writer.WriteInt16(packet.ItemIndex);
    writer.WriteBoolean(packet.ForceAssignToServer);
    return stream.ToArray();
  }

  public static PacketReadResult<InstancedItemPacket> DecodeInstancedItemBody(
    ReadOnlyMemory<byte> body)
  {
    PacketReadResult<SyncItemPacket> decoded = DecodeSyncItemBody(body);
    if (!decoded.Success)
    {
      return PacketReadResult<InstancedItemPacket>.Failed(decoded.Error!);
    }

    SyncItemPacket item = decoded.Packet!;
    return PacketReadResult<InstancedItemPacket>.Succeeded(
      new InstancedItemPacket
      {
        ItemIndex = item.ItemIndex,
        PositionX = item.PositionX,
        PositionY = item.PositionY,
        VelocityX = item.VelocityX,
        VelocityY = item.VelocityY,
        Stack = item.Stack,
        Prefix = item.Prefix,
        StateFlags = item.StateFlags,
        ItemType = item.ItemType,
        Shimmered = item.Shimmered,
        ShimmerTime = item.ShimmerTime,
        EnemyGrabDelayTime = item.EnemyGrabDelayTime
      },
      decoded.Consumed);
  }

  public static byte[] EncodeInstancedItemBody(InstancedItemPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    return EncodeSyncItemBody(new SyncItemPacket
    {
      ItemIndex = packet.ItemIndex,
      PositionX = packet.PositionX,
      PositionY = packet.PositionY,
      VelocityX = packet.VelocityX,
      VelocityY = packet.VelocityY,
      Stack = packet.Stack,
      Prefix = packet.Prefix,
      StateFlags = packet.StateFlags,
      ItemType = packet.ItemType,
      Shimmered = packet.Shimmered,
      ShimmerTime = packet.ShimmerTime,
      EnemyGrabDelayTime = packet.EnemyGrabDelayTime
    });
  }

  public static PacketReadResult<SyncItemPacket> DecodeSyncItemBody(
    ReadOnlyMemory<byte> body)
  {
    var reader = new PacketWireReader(body);
    try
    {
      short itemIndex = reader.ReadInt16();
      float positionX = reader.ReadSingle();
      float positionY = reader.ReadSingle();
      float velocityX = reader.ReadSingle();
      float velocityY = reader.ReadSingle();
      short stack = reader.ReadInt16();
      byte prefix = reader.ReadByte();
      byte flags = reader.ReadByte();
      short itemType = reader.ReadInt16();

      if ((flags & ~KnownFlags) != 0)
      {
        return Invalid<SyncItemPacket>(reader, "StateFlags",
          "Steam item sync contains unknown state flags.");
      }

      bool? shimmered = null;
      float? shimmerTime = null;
      byte? enemyGrabDelayTime = null;
      if ((flags & (1 << 2)) != 0)
      {
        byte shimmeredByte = reader.ReadByte();
        if (shimmeredByte > 1)
        {
          return Invalid<SyncItemPacket>(reader, "Shimmered",
            "Steam shimmered value must be encoded as zero or one.");
        }

        shimmered = shimmeredByte != 0;
        shimmerTime = reader.ReadSingle();
      }

      if ((flags & (1 << 3)) != 0)
      {
        enemyGrabDelayTime = reader.ReadByte();
      }

      reader.RequireFrameEnd();
      if (!IsValidItemState(itemIndex, positionX, positionY, velocityX,
          velocityY, stack, itemType) ||
        (shimmerTime.HasValue && !float.IsFinite(shimmerTime.Value)))
      {
        return Invalid<SyncItemPacket>(reader, "ItemState",
          "Steam item sync contains an invalid slot, item value, or vector.");
      }

      return PacketReadResult<SyncItemPacket>.Succeeded(new SyncItemPacket
      {
        ItemIndex = itemIndex,
        PositionX = positionX,
        PositionY = positionY,
        VelocityX = velocityX,
        VelocityY = velocityY,
        Stack = stack,
        Prefix = prefix,
        StateFlags = flags,
        ItemType = itemType,
        Shimmered = shimmered,
        ShimmerTime = shimmerTime,
        EnemyGrabDelayTime = enemyGrabDelayTime
      }, reader.Position);
    }
    catch (PacketWireTruncationException exception)
    {
      return Failed<SyncItemPacket>(PacketReadStatus.Truncated,
        PacketReadErrorCode.Truncated, reader.Position, exception.Message);
    }
    catch (PacketWireFormatException exception)
    {
      return Failed<SyncItemPacket>(PacketReadStatus.InvalidData,
        exception.Code, reader.Position, exception.Message);
    }
  }

  public static byte[] EncodeSyncItemBody(SyncItemPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!IsValidItemState(packet.ItemIndex, packet.PositionX, packet.PositionY,
          packet.VelocityX, packet.VelocityY, packet.Stack, packet.ItemType) ||
      (packet.StateFlags & ~KnownFlags) != 0 ||
      !HasMatchingExtensions(packet) ||
      (packet.ShimmerTime.HasValue && !float.IsFinite(packet.ShimmerTime.Value)))
    {
      throw new PacketEncodingException(
        "Steam item sync contains invalid base fields or mismatched extension flags.");
    }

    using var stream = new MemoryStream(MaximumItemBytes);
    var writer = new PacketWireWriter(stream, MaximumItemBytes);
    writer.WriteInt16(packet.ItemIndex);
    writer.WriteSingle(packet.PositionX);
    writer.WriteSingle(packet.PositionY);
    writer.WriteSingle(packet.VelocityX);
    writer.WriteSingle(packet.VelocityY);
    writer.WriteInt16(packet.Stack);
    writer.WriteByte(packet.Prefix);
    writer.WriteByte(packet.StateFlags);
    writer.WriteInt16(packet.ItemType);

    if ((packet.StateFlags & (1 << 2)) != 0)
    {
      writer.WriteBoolean(packet.Shimmered!.Value);
      writer.WriteSingle(packet.ShimmerTime!.Value);
    }

    if ((packet.StateFlags & (1 << 3)) != 0)
    {
      writer.WriteByte(packet.EnemyGrabDelayTime!.Value);
    }

    return stream.ToArray();
  }

  public static PacketReadResult<ItemOwnerPacket> DecodeItemOwnerBody(
    ReadOnlyMemory<byte> body)
  {
    var reader = new PacketWireReader(body);
    try
    {
      short itemIndex = reader.ReadInt16();
      byte reservedForPlayer = reader.ReadByte();
      int timeToKeepReservation = Read7BitEncodedInt(reader);
      byte grabDelayPlayer = reader.ReadByte();
      int grabDelayTime = Read7BitEncodedInt(reader);
      float positionX = reader.ReadSingle();
      float positionY = reader.ReadSingle();
      reader.RequireFrameEnd();

      if (itemIndex < 0 || itemIndex >= MaximumItemSlot ||
        !float.IsFinite(positionX) || !float.IsFinite(positionY))
      {
        return Invalid<ItemOwnerPacket>(reader, "ItemOwner",
          "Steam item owner contains an invalid slot or position.");
      }

      return PacketReadResult<ItemOwnerPacket>.Succeeded(new ItemOwnerPacket
      {
        ItemIndex = itemIndex,
        ReservedForPlayer = reservedForPlayer,
        TimeToKeepReservation = timeToKeepReservation,
        GrabDelayPlayer = grabDelayPlayer,
        GrabDelayTime = grabDelayTime,
        PositionX = positionX,
        PositionY = positionY
      }, reader.Position);
    }
    catch (PacketWireTruncationException exception)
    {
      return Failed<ItemOwnerPacket>(PacketReadStatus.Truncated,
        PacketReadErrorCode.Truncated, reader.Position, exception.Message);
    }
    catch (PacketWireFormatException exception)
    {
      return Failed<ItemOwnerPacket>(PacketReadStatus.InvalidData,
        exception.Code, reader.Position, exception.Message);
    }
  }

  public static byte[] EncodeItemOwnerBody(ItemOwnerPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex < 0 || packet.ItemIndex >= MaximumItemSlot ||
      packet.TimeToKeepReservation < 0 || packet.GrabDelayTime < 0 ||
      !float.IsFinite(packet.PositionX) || !float.IsFinite(packet.PositionY))
    {
      throw new PacketEncodingException(
        "Steam item owner contains an invalid slot, timer, or position.");
    }

    using var stream = new MemoryStream(22);
    var writer = new PacketWireWriter(stream, 22);
    writer.WriteInt16(packet.ItemIndex);
    writer.WriteByte(packet.ReservedForPlayer);
    Write7BitEncodedInt(writer, packet.TimeToKeepReservation);
    writer.WriteByte(packet.GrabDelayPlayer);
    Write7BitEncodedInt(writer, packet.GrabDelayTime);
    writer.WriteSingle(packet.PositionX);
    writer.WriteSingle(packet.PositionY);
    return stream.ToArray();
  }

  private static MemoryStream? EncodeSyncItemBodyStream(SyncItemPacket packet)
  {
    byte[] body = EncodeSyncItemBody(packet);
    return new MemoryStream(body, writable: false);
  }

  private static MemoryStream? EncodeItemOwnerBodyStream(ItemOwnerPacket packet)
  {
    byte[] body = EncodeItemOwnerBody(packet);
    return new MemoryStream(body, writable: false);
  }

  private static MemoryStream? EncodeReleaseItemOwnershipBodyStream(
    ReleaseItemOwnershipPacket packet)
  {
    byte[] body = EncodeReleaseItemOwnershipBody(packet);
    return new MemoryStream(body, writable: false);
  }

  private static MemoryStream? EncodeInstancedItemBodyStream(
    InstancedItemPacket packet)
  {
    byte[] body = EncodeInstancedItemBody(packet);
    return new MemoryStream(body, writable: false);
  }

  private static bool IsReplacedMessage(byte messageId) =>
    Array.IndexOf(ReplacedMessageIds, messageId) >= 0;

  private static PacketBinding FindGeneratedBinding(
    PacketBinding[] generated,
    byte messageId,
    PacketDirection direction)
  {
    PacketBinding[] matches = generated
      .Where(binding => binding.MessageId == messageId &&
        binding.Direction == direction)
      .ToArray();
    if (matches.Length != 1)
    {
      throw new InvalidOperationException(
        $"Expected exactly one generated binding for item message {messageId} in {direction} direction.");
    }

    return matches[0];
  }

  private static int Read7BitEncodedInt(PacketWireReader reader)
  {
    uint value = 0;
    for (int shift = 0; shift <= 28; shift += 7)
    {
      byte next = reader.ReadByte();
      if (shift == 28 && next > 0x07)
      {
        throw new PacketWireFormatException(
          "Steam 7-bit integer exceeds Int32.",
          PacketReadErrorCode.InvalidLengthPrefix);
      }

      value |= (uint)(next & 0x7F) << shift;
      if ((next & 0x80) == 0)
      {
        if (shift > 0 && next == 0)
        {
          throw new PacketWireFormatException(
            "Steam 7-bit integer is not minimally encoded.",
            PacketReadErrorCode.InvalidLengthPrefix);
        }

        return checked((int)value);
      }
    }

    throw new PacketWireFormatException(
      "Steam 7-bit integer is longer than five bytes.",
      PacketReadErrorCode.InvalidLengthPrefix);
  }

  private static void Write7BitEncodedInt(PacketWireWriter writer, int value)
  {
    uint remaining = checked((uint)value);
    while (remaining >= 0x80)
    {
      writer.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    writer.WriteByte((byte)remaining);
  }

  private static bool IsValidItemState(
    short itemIndex,
    float positionX,
    float positionY,
    float velocityX,
    float velocityY,
    short stack,
    short itemType)
  {
    return itemIndex >= 0 && itemIndex <= MaximumItemSlot &&
      stack >= 0 && itemType >= 0 &&
      float.IsFinite(positionX) && float.IsFinite(positionY) &&
      float.IsFinite(velocityX) && float.IsFinite(velocityY);
  }

  private static bool HasMatchingExtensions(SyncItemPacket packet)
  {
    bool hasShimmerExtension = (packet.StateFlags & (1 << 2)) != 0;
    bool hasEnemyGrabDelayExtension = (packet.StateFlags & (1 << 3)) != 0;
    return hasShimmerExtension == packet.Shimmered.HasValue &&
      hasShimmerExtension == packet.ShimmerTime.HasValue &&
      hasEnemyGrabDelayExtension == packet.EnemyGrabDelayTime.HasValue;
  }

  private static PacketReadResult<TPacket> Invalid<TPacket>(
    PacketWireReader reader,
    string member,
    string message)
  {
    return PacketReadResult<TPacket>.Failed(new PacketReadError(
      PacketReadStatus.InvalidData,
      PacketReadErrorCode.InvalidCodecData,
      reader.Position,
      member,
      message));
  }

  private static PacketReadResult<TPacket> Failed<TPacket>(
    PacketReadStatus status,
    PacketReadErrorCode code,
    int offset,
    string message)
  {
    return PacketReadResult<TPacket>.Failed(new PacketReadError(
      status,
      code,
      offset,
      null,
      message));
  }
}
