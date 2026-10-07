using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MassWireOperationPacket
{
    public static void Write(
        PacketWireWriter writer,
        short startX,
        short startY,
        short endX,
        short endY,
        byte toolMode)
    {
        writer.WriteInt16(startX);
        writer.WriteInt16(startY);
        writer.WriteInt16(endX);
        writer.WriteInt16(endY);
        writer.WriteByte(toolMode);
    }

    public static (
        short StartX,
        short StartY,
        short EndX,
        short EndY,
        byte ToolMode) Read(PacketWireReader reader) => (StartX: reader.ReadInt16(), StartY: reader.ReadInt16(), EndX: reader.ReadInt16(), EndY: reader.ReadInt16(), ToolMode: reader.ReadByte());
}
