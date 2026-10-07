using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AchievementMessageEventHappenedPacket
{
    public static void Write(PacketWireWriter writer, short eventId)
    {
        writer.WriteInt16(eventId);
    }

    public static short Read(PacketWireReader reader)
    {
        return reader.ReadInt16();
    }
}
