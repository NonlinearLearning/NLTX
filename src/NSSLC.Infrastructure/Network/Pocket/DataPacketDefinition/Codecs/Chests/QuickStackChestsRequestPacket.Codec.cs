using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuickStackChestsRequestPacket
{
    private const int MaximumBodyBytes = ushort.MaxValue - 3;

    public static void Write(PacketWireWriter writer, short[] itemSlots, bool smartStack)
    {
        ArgumentNullException.ThrowIfNull(itemSlots);
        if (itemSlots.Length > (MaximumBodyBytes - sizeof(int) - sizeof(bool)) / sizeof(short))
            throw new PacketWireFormatException("Packet 85 slot count exceeds the maximum frame size.");

        writer.WriteInt32(itemSlots.Length);
        foreach (short itemSlot in itemSlots)
            writer.WriteInt16(itemSlot);
        writer.WriteBoolean(smartStack);
    }

    public static (short[] ItemSlots, bool SmartStack) Read(PacketWireReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || reader.Remaining < sizeof(bool)
            || count > (reader.Remaining - sizeof(bool)) / sizeof(short)
            || reader.Remaining != count * sizeof(short) + sizeof(bool))
        {
            throw new PacketWireFormatException("Packet 85 slot count does not match its bounded body.");
        }

        var itemSlots = new short[count];
        for (int index = 0; index < itemSlots.Length; index++)
            itemSlots[index] = reader.ReadInt16();
        return (itemSlots, reader.ReadBoolean());
    }
}
