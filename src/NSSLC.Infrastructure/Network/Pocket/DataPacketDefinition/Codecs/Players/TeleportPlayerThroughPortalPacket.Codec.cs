using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportPlayerThroughPortalPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        short portalColorIndex,
        float positionX,
        float positionY,
        float velocityX,
        float velocityY)
    {
        writer.WriteByte(player);
        writer.WriteInt16(portalColorIndex);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
        writer.WriteSingle(velocityX);
        writer.WriteSingle(velocityY);
    }

    public static (
        byte Player,
        short PortalColorIndex,
        float PositionX,
        float PositionY,
        float VelocityX,
        float VelocityY) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        short portalColor = reader.ReadInt16();
        return (Player: player, PortalColorIndex: portalColor, PositionX: reader.ReadSingle(), PositionY: reader.ReadSingle(), VelocityX: reader.ReadSingle(), VelocityY: reader.ReadSingle());
    }
}
