using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestTileEntityInteractionPacket
{
    public static void Write(PacketWireWriter writer, int entityId, byte player)
    {
        writer.WriteInt32(entityId);
        writer.WriteByte(player);
    }

    public static (int EntityId, byte Player) Read(PacketWireReader reader)
    {
        int entityId = reader.ReadInt32();
        byte player = reader.ReadByte();
        return (EntityId: entityId, Player: player);
    }
}
