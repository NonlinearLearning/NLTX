using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class NPCBuffsPacket
{
    public static void Write(PacketWireWriter writer, short npcIndex, IReadOnlyList<PacketBuffTime> buffs)
    {
        writer.WriteInt16(npcIndex);
        foreach (PacketBuffTime buff in buffs)
        {
            if (buff.Type == 0 || buff.Time == 0)
                throw new PacketWireFormatException("Packet 54 entries require nonzero buff type and time.");
            writer.WriteUInt16(buff.Type);
            writer.WriteUInt16(buff.Time);
        }

        writer.WriteUInt16(0);
    }

    public static (short NpcIndex, IReadOnlyList<PacketBuffTime> Buffs) Read(PacketWireReader reader)
    {
        short npcIndex = reader.ReadInt16();
        var buffs = new List<PacketBuffTime>();
        while (true)
        {
            ushort type = reader.ReadUInt16();
            if (type == 0)
                break;
            ushort time = reader.ReadUInt16();
            buffs.Add(new PacketBuffTime(type, time));
        }

        return (NpcIndex: npcIndex, Buffs: buffs);
    }
}
