using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown66Packet
{
    public static void Write(PacketWireWriter writer, byte player, short lifeAmount)
    {
        writer.WriteByte(player);
        writer.WriteInt16(lifeAmount);
    }

    public static (byte Player, short LifeAmount) Read(PacketWireReader reader) => (Player: reader.ReadByte(), LifeAmount: reader.ReadInt16());
}
