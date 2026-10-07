using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UniqueTownNPCInfoSyncRequestPacket
{
    public static void Write(PacketWireWriter writer, short npcIndex) => writer.WriteInt16(npcIndex);
    public static short Read(PacketWireReader reader) => reader.ReadInt16();
}
