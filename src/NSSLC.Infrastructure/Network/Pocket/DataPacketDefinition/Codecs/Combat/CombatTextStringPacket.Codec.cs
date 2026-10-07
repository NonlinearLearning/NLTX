using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CombatTextStringPacket
{
    public static void Write(
        PacketWireWriter writer,
        float x,
        float y,
        PacketRgb color,
        NetworkText text)
    {
        writer.WriteSingle(x);
        writer.WriteSingle(y);
        writer.WriteByte(color.Red);
        writer.WriteByte(color.Green);
        writer.WriteByte(color.Blue);
        NetworkTextCodec.Write(writer, text);
    }

    public static (float X, float Y, PacketRgb Color, NetworkText Text) Read(PacketWireReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        var color = new PacketRgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        NetworkText text = NetworkTextCodec.Read(reader);
        return (X: x, Y: y, Color: color, Text: text);
    }
}
