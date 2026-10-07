using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TravelMerchantItemsPacket
{
    public static void Write(PacketWireWriter writer, IReadOnlyList<short> items, int slotCount)
    {
        RequireSlotCount(slotCount);
        if (items.Count != slotCount)
            throw new PacketWireFormatException($"Packet 72 requires exactly {slotCount} item slots.");
        foreach (short item in items)
            writer.WriteInt16(item);
    }

    public static IReadOnlyList<short> Read(PacketWireReader reader, int slotCount)
    {
        RequireSlotCount(slotCount);
        var items = new short[slotCount];
        for (int index = 0; index < items.Length; index++)
            items[index] = reader.ReadInt16();
        return items;
    }

    private static void RequireSlotCount(int slotCount)
    {
        if (slotCount < 0 || (long)slotCount * sizeof(short) > PacketFrameLimits.MaximumPayloadBytes)
            throw new ArgumentOutOfRangeException(nameof(slotCount));
    }
}
