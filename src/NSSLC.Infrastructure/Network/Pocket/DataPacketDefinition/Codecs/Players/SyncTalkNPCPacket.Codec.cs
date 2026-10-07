using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncTalkNPCPacket
{
    public static void Write(PacketWireWriter writer, byte player, short npcIndex)
    {
        writer.WriteByte(player);
        writer.WriteInt16(npcIndex);
    }

    public static (byte Player, short NpcIndex) Read(PacketWireReader reader) => (Player: reader.ReadByte(), NpcIndex: reader.ReadInt16());
}
