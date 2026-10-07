using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown42Packet
{
    public static void Write(PacketWireWriter writer, byte player, short mana, short maximumMana)
    {
        writer.WriteByte(player);
        writer.WriteInt16(mana);
        writer.WriteInt16(maximumMana);
    }

    public static (byte Player, short Mana, short MaximumMana) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Mana: reader.ReadInt16(), MaximumMana: reader.ReadInt16());
}
