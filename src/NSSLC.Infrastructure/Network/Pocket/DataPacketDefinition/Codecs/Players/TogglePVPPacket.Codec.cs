using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TogglePVPPacket
{
    public static void Write(PacketWireWriter writer, byte player, bool hostile)
    {
        writer.WriteByte(player);
        writer.WriteBoolean(hostile);
    }

    public static (byte Player, bool Hostile) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Hostile: reader.ReadBoolean());
}
