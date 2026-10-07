using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpectatePlayerPacket
{
    public static void Write(PacketWireWriter writer, byte player, short targetPlayer)
    {
        writer.WriteByte(player);
        writer.WriteInt16(targetPlayer);
    }

    public static (byte Player, short TargetPlayer) Read(PacketWireReader reader)
    {
        return (Player: reader.ReadByte(), TargetPlayer: reader.ReadInt16());
    }
}
