using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerChestIndexPacket
{
    public static void Write(PacketWireWriter writer, byte player, short chestIndex)
    {
        writer.WriteByte(player);
        writer.WriteInt16(chestIndex);
    }

    public static (byte Player, short ChestIndex) Read(PacketWireReader reader) => (Player: reader.ReadByte(), ChestIndex: reader.ReadInt16());
}
