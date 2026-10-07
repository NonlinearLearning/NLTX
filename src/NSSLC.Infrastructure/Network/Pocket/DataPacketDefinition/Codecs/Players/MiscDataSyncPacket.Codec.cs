using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MiscDataSyncPacket
{
    public static void Write(PacketWireWriter writer, byte player, byte action)
    {
        writer.WriteByte(player);
        writer.WriteByte(action);
    }

    public static (byte Player, byte Action) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Action: reader.ReadByte());
}
