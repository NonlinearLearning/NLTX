using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemPositionPacket
{
    private const short MaximumItemSlots = 400;

    public static void Write(PacketWireWriter writer, short itemIndex, PacketVector2 position)
    {
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots ||
            !float.IsFinite(position.X) || !float.IsFinite(position.Y))
            throw new PacketWireFormatException("Packet 160 contains an invalid world-item slot or position.");

        writer.WriteInt16(itemIndex);
        writer.WriteSingle(position.X);
        writer.WriteSingle(position.Y);
    }

    public static (short ItemIndex, PacketVector2 Position) Read(PacketWireReader reader)
    {
        short itemIndex = reader.ReadInt16();
        var position = new PacketVector2(reader.ReadSingle(), reader.ReadSingle());
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots ||
            !float.IsFinite(position.X) || !float.IsFinite(position.Y))
            throw new PacketWireFormatException("Packet 160 contains an invalid world-item slot or position.");

        return (ItemIndex: itemIndex, Position: position);
    }
}
