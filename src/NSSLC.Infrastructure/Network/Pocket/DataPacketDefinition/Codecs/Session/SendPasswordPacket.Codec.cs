using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SendPasswordPacket
{
    public static void Write(PacketWireWriter writer, string password)
    {
        writer.WriteString(password);
    }

    public static string Read(PacketWireReader reader)
    {
        int start = reader.Position;
        string password = reader.ReadString();
        return password;
    }
}
