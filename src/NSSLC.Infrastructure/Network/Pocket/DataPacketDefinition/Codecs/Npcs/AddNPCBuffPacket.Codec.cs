using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AddNPCBuffPacket
{
    public static void Write(PacketWireWriter writer, short npcIndex, ushort buffType, short buffTime)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteUInt16(buffType);
        writer.WriteInt16(buffTime);
    }

    public static (short NpcIndex, ushort BuffType, short BuffTime) Read(PacketWireReader reader) => (NpcIndex: reader.ReadInt16(), BuffType: reader.ReadUInt16(), BuffTime: reader.ReadInt16());
}
