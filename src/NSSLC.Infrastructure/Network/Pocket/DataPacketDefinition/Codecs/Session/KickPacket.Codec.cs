using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class KickPacket
{
    public static void Write(PacketWireWriter writer, NetworkText text) => NetworkTextCodec.Write(writer, text);
    public static NetworkText Read(PacketWireReader reader) => NetworkTextCodec.Read(reader);
}
