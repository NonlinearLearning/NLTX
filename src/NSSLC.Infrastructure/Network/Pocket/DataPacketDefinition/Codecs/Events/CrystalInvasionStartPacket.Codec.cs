using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CrystalInvasionStartPacket
{
    public static void Write(PacketWireWriter writer, short x, short y)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
    }

    public static (short X, short Y) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16());
}
