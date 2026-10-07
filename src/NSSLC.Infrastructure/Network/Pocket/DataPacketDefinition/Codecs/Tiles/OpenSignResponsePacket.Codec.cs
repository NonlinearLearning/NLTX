using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class OpenSignResponsePacket
{
    public static void Write(
        PacketWireWriter writer,
        short signIndex,
        short x,
        short y,
        string text,
        byte player,
        byte flags)
    {
        writer.WriteInt16(signIndex);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteString(text);
        writer.WriteByte(player);
        writer.WriteByte(flags);
    }

    public static (
        short SignIndex,
        short X,
        short Y,
        string Text,
        byte Player,
        byte Flags) Read(PacketWireReader reader)
    {
        short sign = reader.ReadInt16();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        string text = reader.ReadString();
        byte player = reader.ReadByte();
        byte flags = reader.ReadByte();
        return (SignIndex: sign, X: x, Y: y, Text: text, Player: player, Flags: flags);
    }
}
