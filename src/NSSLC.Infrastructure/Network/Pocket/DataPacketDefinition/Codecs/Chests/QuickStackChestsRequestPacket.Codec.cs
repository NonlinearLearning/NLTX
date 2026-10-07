using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuickStackChestsRequestPacket
{
    public static void Write(PacketWireWriter writer, bool smartStack) => writer.WriteBoolean(smartStack);
    public static bool Read(PacketWireReader reader) => reader.ReadBoolean();
}
