using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class FishOutNPCPacket
{
    public static void Write(PacketWireWriter writer, ushort x, ushort y, short npcType)
    {
        writer.WriteUInt16(x);
        writer.WriteUInt16(y);
        writer.WriteInt16(npcType);
    }

    public static (ushort X, ushort Y, short NpcType) Read(PacketWireReader reader)
    {
        ushort x = reader.ReadUInt16();
        ushort y = reader.ReadUInt16();
        short npcType = reader.ReadInt16();
        return (X: x, Y: y, NpcType: npcType);
    }
}
