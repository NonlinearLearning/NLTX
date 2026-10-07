using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class Unknown62Packet
{
    public static void Write(PacketWireWriter writer, byte player, byte dodgeType)
    {
        writer.WriteByte(player);
        writer.WriteByte(dodgeType);
    }

    public static (byte Player, byte DodgeType) Read(PacketWireReader reader) => (Player: reader.ReadByte(), DodgeType: reader.ReadByte());
}
