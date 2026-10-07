using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class StatusTextSizePacket
{
    public static void Write(PacketWireWriter writer, int value, NetworkText text, byte flags)
    {
        writer.WriteInt32(value);
        NetworkTextCodec.Write(writer, text);
        writer.WriteByte(flags);
    }

    public static (int Value, NetworkText Text, byte Flags) Read(PacketWireReader reader)
    {
        int value = reader.ReadInt32();
        NetworkText text = NetworkTextCodec.Read(reader);
        byte flags = reader.ReadByte();
        return (Value: value, Text: text, Flags: flags);
    }
}
