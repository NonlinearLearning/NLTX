using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerInfoPacket
{
    public static void Write(PacketWireWriter writer, byte player, bool accepted)
    {
        writer.WriteByte(player);
        writer.WriteBoolean(accepted);
    }

    public static (byte Player, bool Accepted) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Accepted: reader.ReadBoolean());
}
