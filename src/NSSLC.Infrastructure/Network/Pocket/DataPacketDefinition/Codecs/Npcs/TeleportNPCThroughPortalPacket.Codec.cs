using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportNPCThroughPortalPacket
{
    public static void Write(
        PacketWireWriter writer,
        ushort npcIndex,
        short portalColorIndex,
        float positionX,
        float positionY,
        float velocityX,
        float velocityY)
    {
        writer.WriteUInt16(npcIndex);
        writer.WriteInt16(portalColorIndex);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
        writer.WriteSingle(velocityX);
        writer.WriteSingle(velocityY);
    }

    public static (
        ushort NpcIndex,
        short PortalColorIndex,
        float PositionX,
        float PositionY,
        float VelocityX,
        float VelocityY) Read(PacketWireReader reader)
    {
        ushort npcIndex = reader.ReadUInt16();
        short portalColor = reader.ReadInt16();
        return (NpcIndex: npcIndex, PortalColorIndex: portalColor, PositionX: reader.ReadSingle(), PositionY: reader.ReadSingle(), VelocityX: reader.ReadSingle(), VelocityY: reader.ReadSingle());
    }
}
