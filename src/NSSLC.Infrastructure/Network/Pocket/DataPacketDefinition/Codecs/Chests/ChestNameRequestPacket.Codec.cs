using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestNameRequestPacket
{
    public static void Write(PacketWireWriter writer, short chestIndex, short x, short y)
    {
        writer.WriteInt16(chestIndex);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
    }

    public static (short ChestIndex, short X, short Y) Read(PacketWireReader reader) => (ChestIndex: reader.ReadInt16(), X: reader.ReadInt16(), Y: reader.ReadInt16());
}
