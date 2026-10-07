using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerHealPacket
{
    public static void Write(PacketWireWriter writer, byte player, short healAmount)
    {
        writer.WriteByte(player);
        writer.WriteInt16(healAmount);
    }

    public static (byte Player, short HealAmount) Read(PacketWireReader reader) => (Player: reader.ReadByte(), HealAmount: reader.ReadInt16());
}
