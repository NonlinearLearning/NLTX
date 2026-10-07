using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class HostTokenPacket
{
    public static void Write(PacketWireWriter writer, string hostToken)
    {
        writer.WriteString(hostToken);
    }

    public static string Read(PacketWireReader reader)
    {
        return reader.ReadString();
    }
}
