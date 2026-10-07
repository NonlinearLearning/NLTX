using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerLifeManaPacket
{
    public static void Write(PacketWireWriter writer, byte player, short life, short maximumLife)
    {
        writer.WriteByte(player);
        writer.WriteInt16(life);
        writer.WriteInt16(maximumLife);
    }

    public static (byte Player, short Life, short MaximumLife) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Life: reader.ReadInt16(), MaximumLife: reader.ReadInt16());
}
