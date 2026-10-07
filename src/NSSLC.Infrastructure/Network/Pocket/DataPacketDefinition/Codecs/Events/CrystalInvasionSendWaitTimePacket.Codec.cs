using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CrystalInvasionSendWaitTimePacket
{
    public static void Write(PacketWireWriter writer, int waitTime)
    {
        writer.WriteInt32(waitTime);
    }

    public static int Read(PacketWireReader reader) => reader.ReadInt32();
}
