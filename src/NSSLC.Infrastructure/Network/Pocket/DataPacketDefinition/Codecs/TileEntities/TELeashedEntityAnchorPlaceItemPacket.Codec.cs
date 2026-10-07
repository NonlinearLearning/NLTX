using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TELeashedEntityAnchorPlaceItemPacket
{
    public static void Write(PacketWireWriter writer, short x, short y, short itemType)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(itemType);
    }

    public static (short X, short Y, short ItemType) Read(PacketWireReader reader)
    {
        return (X: reader.ReadInt16(), Y: reader.ReadInt16(), ItemType: reader.ReadInt16());
    }
}
