using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown60Packet
{
    public static void Write(
        PacketWireWriter writer,
        short npcIndex,
        short roomX,
        short roomY,
        byte action)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteInt16(roomX);
        writer.WriteInt16(roomY);
        writer.WriteByte(action);
    }

    public static (short NpcIndex, short RoomX, short RoomY, byte Action) Read(PacketWireReader reader) => (NpcIndex: reader.ReadInt16(), RoomX: reader.ReadInt16(), RoomY: reader.ReadInt16(), Action: reader.ReadByte());
}
