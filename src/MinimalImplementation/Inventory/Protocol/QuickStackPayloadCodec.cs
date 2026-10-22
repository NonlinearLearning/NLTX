using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Inventory.Protocol;

public static class QuickStackPayloadCodec
{
  public static QuickStackRequestPayload ReadRequest(PacketWireReader reader, int playerSlotCount)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentOutOfRangeException.ThrowIfNegative(playerSlotCount);
    int count = reader.ReadInt32();
    if (count < 0
      || count > playerSlotCount
      || reader.Remaining < sizeof(byte)
      || count > (reader.Remaining - sizeof(byte)) / sizeof(short))
    {
      throw new PacketWireFormatException(
        "The QuickStack slot count is outside the remaining packet payload.",
        PacketReadErrorCode.LengthOutOfRange);
    }

    var slotIds = new short[count];
    var seen = new HashSet<short>();
    for (int index = 0; index < count; index++)
    {
      short slotId = reader.ReadInt16();
      if (slotId < 0 || slotId >= playerSlotCount || !seen.Add(slotId))
      {
        throw new PacketWireFormatException(
          "The QuickStack slot list contains a negative or duplicate slot id.");
      }

      slotIds[index] = slotId;
    }

    bool smartStack = reader.ReadBoolean();
    reader.RequireFrameEnd();
    return new QuickStackRequestPayload(slotIds, smartStack);
  }

  public static QuickStackBlockedChestPayload ReadBlockedChests(
    PacketWireReader reader,
    int chestIndexExclusive)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chestIndexExclusive);

    int count = reader.ReadInt32();
    if (count < 0 || count > chestIndexExclusive || count > reader.Remaining / sizeof(ushort))
    {
      throw new PacketWireFormatException(
        "The QuickStack blocked chest count is outside the configured world or payload bound.",
        PacketReadErrorCode.LengthOutOfRange);
    }

    var chestIds = new ushort[count];
    for (int index = 0; index < count; index++)
    {
      ushort chestId = reader.ReadUInt16();
      if (chestId >= chestIndexExclusive)
      {
        throw new PacketWireFormatException(
          "The QuickStack blocked chest list contains an index outside the world bound.");
      }

      chestIds[index] = chestId;
    }

    reader.RequireFrameEnd();
    return new QuickStackBlockedChestPayload(chestIds);
  }

  public static void WriteRequest(
    PacketWireWriter writer,
    QuickStackRequestPayload payload,
    int playerSlotCount)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(payload);
    ArgumentOutOfRangeException.ThrowIfNegative(playerSlotCount);
    ValidateSlotIds(payload.SlotIds, playerSlotCount);
    writer.WriteInt32(payload.SlotIds.Count);
    foreach (short slotId in payload.SlotIds)
    {
      writer.WriteInt16(slotId);
    }

    writer.WriteBoolean(payload.SmartStack);
  }

  public static void WriteBlockedChests(
    PacketWireWriter writer,
    QuickStackBlockedChestPayload payload,
    int chestIndexExclusive)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(payload);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chestIndexExclusive);
    if (payload.ChestIds.Count > chestIndexExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(payload), "The blocked chest list exceeds the world bound.");
    }

    writer.WriteInt32(payload.ChestIds.Count);
    foreach (ushort chestId in payload.ChestIds)
    {
      if (chestId >= chestIndexExclusive)
      {
        throw new ArgumentOutOfRangeException(nameof(payload), "A blocked chest index exceeds the world bound.");
      }

      writer.WriteUInt16(chestId);
    }
  }

  private static void ValidateSlotIds(IReadOnlyList<short> slotIds, int playerSlotCount)
  {
    var seen = new HashSet<short>();
    foreach (short slotId in slotIds)
    {
      if (slotId < 0 || slotId >= playerSlotCount || !seen.Add(slotId))
      {
        throw new ArgumentException(
          "The QuickStack slot list must contain unique, non-negative slot ids.",
          nameof(slotIds));
      }
    }
  }
}
