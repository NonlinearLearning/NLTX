using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpecialFXPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte effectType,
        int x,
        int y,
        byte parameter,
        short style,
        byte flag)
    {
        writer.WriteByte(effectType);
        writer.WriteInt32(x);
        writer.WriteInt32(y);
        writer.WriteByte(parameter);
        writer.WriteInt16(style);
        writer.WriteByte(flag);
    }

    public static (
        byte EffectType,
        int X,
        int Y,
        byte Parameter,
        short Style,
        byte Flag) Read(PacketWireReader reader)
    {
        byte effectType = reader.ReadByte();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        byte parameter = reader.ReadByte();
        short style = reader.ReadInt16();
        byte flag = reader.ReadByte();
        return (EffectType: effectType, X: x, Y: y, Parameter: parameter, Style: style, Flag: flag);
    }
}
