using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class HelloPacket
{
    public static void Write(PacketWireWriter writer, string version)
    {
        writer.WriteString(version);
    }

    public static string Read(PacketWireReader reader)
    {
        int start = reader.Position;
        string version = reader.ReadString();
        return version;
    }
}
