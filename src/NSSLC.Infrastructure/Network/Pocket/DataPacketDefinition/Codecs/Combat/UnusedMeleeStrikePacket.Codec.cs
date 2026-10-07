using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UnusedMeleeStrikePacket
{
    public static void Write(PacketWireWriter writer, short npcIndex, byte player)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteByte(player);
    }

    public static (short NpcIndex, byte Player) Read(PacketWireReader reader) => (NpcIndex: reader.ReadInt16(), Player: reader.ReadByte());
}
