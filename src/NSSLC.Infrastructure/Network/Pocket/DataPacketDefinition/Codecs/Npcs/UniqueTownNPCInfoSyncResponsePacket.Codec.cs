using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UniqueTownNPCInfoSyncResponsePacket
{
    public static void Write(PacketWireWriter writer, short npcIndex, string givenName, int variation)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteString(givenName);
        writer.WriteInt32(variation);
    }

    public static (short NpcIndex, string GivenName, int Variation) Read(PacketWireReader reader) => (NpcIndex: reader.ReadInt16(), GivenName: reader.ReadString(), Variation: reader.ReadInt32());
}
