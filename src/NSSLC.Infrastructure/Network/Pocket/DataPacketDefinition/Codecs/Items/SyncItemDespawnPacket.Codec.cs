using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemDespawnPacket
{
    private const short MaximumItemSlots = 400;

    public static void Write(PacketWireWriter writer, short itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots)
            throw new PacketWireFormatException("Packet 151 item slot is outside the Steam world-item range.");

        writer.WriteInt16(itemIndex);
    }

    public static short Read(PacketWireReader reader)
    {
        short itemIndex = reader.ReadInt16();
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots)
            throw new PacketWireFormatException("Packet 151 item slot is outside the Steam world-item range.");

        return itemIndex;
    }
}
