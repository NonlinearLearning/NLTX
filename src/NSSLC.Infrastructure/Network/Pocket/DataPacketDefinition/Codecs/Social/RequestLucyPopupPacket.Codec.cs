using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestLucyPopupPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte messageSource,
        byte variation,
        PacketVector2 velocity,
        int positionX,
        int positionY)
    {
        writer.WriteByte(messageSource);
        writer.WriteByte(variation);
        writer.WriteSingle(velocity.X);
        writer.WriteSingle(velocity.Y);
        writer.WriteInt32(positionX);
        writer.WriteInt32(positionY);
    }

    public static (
        byte MessageSource,
        byte Variation,
        PacketVector2 Velocity,
        int PositionX,
        int PositionY) Read(PacketWireReader reader)
    {
        byte source = reader.ReadByte();
        byte variation = reader.ReadByte();
        var velocity = new PacketVector2(reader.ReadSingle(), reader.ReadSingle());
        int positionX = reader.ReadInt32();
        int positionY = reader.ReadInt32();
        return (MessageSource: source, Variation: variation, Velocity: velocity,
            PositionX: positionX, PositionY: positionY);
    }
}
