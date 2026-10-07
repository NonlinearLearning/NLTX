using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown57Packet
{
    public static void Write(PacketWireWriter writer, byte good, byte evil, byte blood)
    {
        writer.WriteByte(good);
        writer.WriteByte(evil);
        writer.WriteByte(blood);
    }

    public static (byte Good, byte Evil, byte Blood) Read(PacketWireReader reader)
    {
        return (Good: reader.ReadByte(), Evil: reader.ReadByte(), Blood: reader.ReadByte());
    }
}
