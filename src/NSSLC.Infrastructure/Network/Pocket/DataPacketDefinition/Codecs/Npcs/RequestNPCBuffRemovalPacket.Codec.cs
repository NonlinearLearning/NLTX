using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestNPCBuffRemovalPacket
{
    public static void Write(PacketWireWriter writer, short npcIndex, ushort buffType)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteUInt16(buffType);
    }

    public static (short NpcIndex, ushort BuffType) Read(PacketWireReader reader)
    {
        return (NpcIndex: reader.ReadInt16(), BuffType: reader.ReadUInt16());
    }
}
