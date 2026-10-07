using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemFrameTryPlacingPacket
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        short itemType,
        byte prefix,
        short stack)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(itemType);
        writer.WriteByte(prefix);
        writer.WriteInt16(stack);
    }

    public static (
        short X,
        short Y,
        short ItemType,
        byte Prefix,
        short Stack) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16(), ItemType: reader.ReadInt16(), Prefix: reader.ReadByte(), Stack: reader.ReadInt16());
}
