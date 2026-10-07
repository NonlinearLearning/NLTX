using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerActivePacket
{
    public static void Write(PacketWireWriter writer, byte player, byte activeState)
    {
        writer.WriteByte(player);
        writer.WriteByte(activeState);
    }

    public static (byte Player, byte ActiveState) Read(PacketWireReader reader) => (Player: reader.ReadByte(), ActiveState: reader.ReadByte());
}
