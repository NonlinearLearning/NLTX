using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class NPCDebuffDamagePacket
{
    public static void Write(PacketWireWriter writer, byte npcIndex, short buffType)
    {
        writer.WriteByte(npcIndex);
        writer.WriteInt16(buffType);
    }

    public static (byte NpcIndex, short BuffType) Read(PacketWireReader reader)
    {
        return (NpcIndex: reader.ReadByte(), BuffType: reader.ReadInt16());
    }
}
