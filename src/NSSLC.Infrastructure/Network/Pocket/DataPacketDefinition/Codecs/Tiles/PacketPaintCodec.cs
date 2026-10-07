using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class PacketPaintCodec
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        byte paintOrCoating,
        byte coatingMode)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteByte(paintOrCoating);
        writer.WriteByte(coatingMode);
    }

    public static (short X, short Y, byte PaintOrCoating, byte CoatingMode) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16(), PaintOrCoating: reader.ReadByte(), CoatingMode: reader.ReadByte());
}
