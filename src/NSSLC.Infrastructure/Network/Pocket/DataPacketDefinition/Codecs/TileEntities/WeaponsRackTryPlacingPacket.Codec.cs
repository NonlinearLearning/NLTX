using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class WeaponsRackTryPlacingPacket
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
        short Stack) Read(PacketWireReader reader)
    {
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        short type = reader.ReadInt16();
        byte prefix = reader.ReadByte();
        short stack = reader.ReadInt16();
        return (X: x, Y: y, ItemType: type, Prefix: prefix, Stack: stack);
    }
}
