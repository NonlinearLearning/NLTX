using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown68Packet
{
    public static void Write(PacketWireWriter writer, string clientUuid)
    {
        writer.WriteString(clientUuid);
    }

    public static string Read(PacketWireReader reader)
    {
        int start = reader.Position;
        string clientUuid = reader.ReadString();
        return clientUuid;
    }
}
