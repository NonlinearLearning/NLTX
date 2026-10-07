using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AchievementMessageNPCKilledPacket
{
    public static void Write(PacketWireWriter writer, short npcNetId)
    {
        writer.WriteInt16(npcNetId);
    }

    public static short Read(PacketWireReader reader)
    {
        return reader.ReadInt16();
    }
}
