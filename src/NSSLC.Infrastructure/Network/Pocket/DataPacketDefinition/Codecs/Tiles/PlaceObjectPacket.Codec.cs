using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlaceObjectPacket
{
    public static void Write(
        PacketWireWriter writer,
        short x,
        short y,
        short objectType,
        short style,
        byte alternate,
        sbyte random,
        bool directionPositive)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(objectType);
        writer.WriteInt16(style);
        writer.WriteByte(alternate);
        writer.WriteSByte(random);
        writer.WriteBoolean(directionPositive);
    }

    public static (
        short X,
        short Y,
        short ObjectType,
        short Style,
        byte Alternate,
        sbyte Random,
        bool DirectionPositive) Read(PacketWireReader reader)
    {
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        short objectType = reader.ReadInt16();
        short style = reader.ReadInt16();
        byte alternate = reader.ReadByte();
        sbyte random = reader.ReadSByte();
        bool directionPositive = reader.ReadBoolean();
        return (X: x, Y: y, ObjectType: objectType, Style: style, Alternate: alternate, Random: random, DirectionPositive: directionPositive);
    }
}
