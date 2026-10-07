using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TEDisplayDollDataSyncPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        int entityId,
        byte itemIndex,
        byte command,
        PacketTileEntityItem? item,
        byte? pose)
    {
        writer.WriteByte(player);
        writer.WriteInt32(entityId);
        writer.WriteByte(itemIndex);
        writer.WriteByte(command);
        if (command == 2)
        {
            if (item is not null || pose is null)
                throw new PacketWireFormatException("Packet 121 command 2 requires a pose byte and no item payload.");
            writer.WriteByte(pose.Value);
        }
        else
        {
            if (item is not PacketTileEntityItem itemValue || pose is not null)
                throw new PacketWireFormatException("Packet 121 non-pose commands require an item payload and no pose byte.");
            WriteItem(writer, itemValue);
        }
    }

    public static (
        byte Player,
        int EntityId,
        byte ItemIndex,
        byte Command,
        PacketTileEntityItem? Item,
        byte? Pose) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        int entityId = reader.ReadInt32();
        byte itemIndex = reader.ReadByte();
        byte command = reader.ReadByte();
        PacketTileEntityItem? item = null;
        byte? pose = null;
        if (command == 2)
            pose = reader.ReadByte();
        else
            item = ReadItem(reader);
        return (Player: player, EntityId: entityId, ItemIndex: itemIndex, Command: command, Item: item, Pose: pose);
    }

    private static void WriteItem(PacketWireWriter writer, PacketTileEntityItem item)
    {
        writer.WriteUInt16(item.Type);
        writer.WriteUInt16(item.Stack);
        writer.WriteByte(item.Prefix);
    }

    private static PacketTileEntityItem ReadItem(PacketWireReader reader) => new(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadByte());
}
