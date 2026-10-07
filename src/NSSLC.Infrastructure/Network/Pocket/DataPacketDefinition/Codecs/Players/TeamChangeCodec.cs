using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class TeamChangeCodec
{
    public static void Write(PacketWireWriter writer, byte player, byte team)
    {
        writer.WriteByte(player);
        writer.WriteByte(team);
    }

    public static (byte Player, byte Team) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Team: reader.ReadByte());
}
