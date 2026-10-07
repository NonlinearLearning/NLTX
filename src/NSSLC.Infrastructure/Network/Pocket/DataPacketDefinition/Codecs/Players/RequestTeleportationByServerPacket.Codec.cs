using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestTeleportationByServerPacket
{
    public static void Write(PacketWireWriter writer, byte teleportationKind)
    {
        writer.WriteByte(teleportationKind);
    }

    public static byte Read(PacketWireReader reader) => reader.ReadByte();
}
