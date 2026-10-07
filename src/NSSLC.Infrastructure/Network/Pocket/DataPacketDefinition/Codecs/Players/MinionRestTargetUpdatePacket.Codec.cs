using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MinionRestTargetUpdatePacket
{
    public static void Write(PacketWireWriter writer, byte player, float targetX, float targetY)
    {
        writer.WriteByte(player);
        writer.WriteSingle(targetX);
        writer.WriteSingle(targetY);
    }

    public static (byte Player, float TargetX, float TargetY) Read(PacketWireReader reader) => (Player: reader.ReadByte(), TargetX: reader.ReadSingle(), TargetY: reader.ReadSingle());
}
