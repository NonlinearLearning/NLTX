using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemUseSoundPacket
{
    public static void Write(PacketWireWriter writer, byte player)
    {
        writer.WriteByte(player);
    }

    public static byte Read(PacketWireReader reader)
    {
        return reader.ReadByte();
    }
}
