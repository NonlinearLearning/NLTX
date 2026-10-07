using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MassWireOperationPayPacket
{
    public static void Write(PacketWireWriter writer, short itemType, short count, byte player)
    {
        writer.WriteInt16(itemType);
        writer.WriteInt16(count);
        writer.WriteByte(player);
    }

    public static (short ItemType, short Count, byte Player) Read(PacketWireReader reader) => (ItemType: reader.ReadInt16(), Count: reader.ReadInt16(), Player: reader.ReadByte());
}
