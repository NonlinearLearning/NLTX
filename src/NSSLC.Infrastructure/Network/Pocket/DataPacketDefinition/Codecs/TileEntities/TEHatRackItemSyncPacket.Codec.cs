using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TEHatRackItemSyncPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        int entityId,
        byte encodedSlot,
        PacketTileEntityItem item)
    {
        writer.WriteByte(player);
        writer.WriteInt32(entityId);
        writer.WriteByte(encodedSlot);
        writer.WriteUInt16(item.Type);
        writer.WriteUInt16(item.Stack);
        writer.WriteByte(item.Prefix);
    }

    public static (byte Player, int EntityId, byte EncodedSlot, PacketTileEntityItem Item) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        int entityId = reader.ReadInt32();
        byte slot = reader.ReadByte();
        var item = new PacketTileEntityItem(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadByte());
        return (Player: player, EntityId: entityId, EncodedSlot: slot, Item: item);
    }
}
