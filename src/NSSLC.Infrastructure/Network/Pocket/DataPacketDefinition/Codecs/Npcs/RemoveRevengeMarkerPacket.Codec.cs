using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RemoveRevengeMarkerPacket
{
    public static void Write(PacketWireWriter writer, int markerId)
    {
        writer.WriteInt32(markerId);
    }

    public static int Read(PacketWireReader reader)
    {
        return reader.ReadInt32();
    }
}
