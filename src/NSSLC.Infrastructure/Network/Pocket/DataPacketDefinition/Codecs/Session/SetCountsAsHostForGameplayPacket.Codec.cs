using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SetCountsAsHostForGameplayPacket
{
    public static void Write(PacketWireWriter writer, byte player, bool countsAsHost)
    {
        writer.WriteByte(player);
        writer.WriteBoolean(countsAsHost);
    }

    public static (byte Player, bool CountsAsHost) Read(PacketWireReader reader)
    {
        return (Player: reader.ReadByte(), CountsAsHost: reader.ReadBoolean());
    }
}
