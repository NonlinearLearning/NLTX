using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileFrameSectionPacket
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        short width,
        short height)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(width);
        writer.WriteInt16(height);
    }

    public static (short X, short Y, short Width, short Height) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16(), Width: reader.ReadInt16(), Height: reader.ReadInt16());
}
