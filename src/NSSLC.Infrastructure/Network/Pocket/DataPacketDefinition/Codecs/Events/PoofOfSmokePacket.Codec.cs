using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PoofOfSmokePacket
{
    public static void Write(PacketWireWriter writer, uint packedPosition)
    {
        writer.WriteUInt32(packedPosition);
    }

    public static uint Read(PacketWireReader reader) => reader.ReadUInt32();
}
