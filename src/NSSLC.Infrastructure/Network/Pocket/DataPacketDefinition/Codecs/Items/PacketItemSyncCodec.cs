using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class PacketItemSyncCodec
{
    public static void Write(PacketWireWriter writer, PacketItemSyncData packet)
    {
        WriteFields(writer, packet.ItemIndex, packet.PositionX, packet.PositionY, packet.VelocityX, packet.VelocityY, packet.Stack, packet.Prefix, packet.StateFlags, packet.ItemType);
    }

    public static PacketItemSyncData Read(PacketWireReader reader)
    {
        var fields = ReadFields(reader);
        return new PacketItemSyncData(fields.ItemIndex, fields.PositionX, fields.PositionY, fields.VelocityX, fields.VelocityY, fields.Stack, fields.Prefix, fields.StateFlags, fields.ItemType);
    }

    public static void WriteFields(
        PacketWireWriter writer,
        short itemIndex,
        float positionX,
        float positionY,
        float velocityX,
        float velocityY,
        short stack,
        byte prefix,
        byte stateFlags,
        short itemType)
    {
        writer.WriteInt16(itemIndex);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
        writer.WriteSingle(velocityX);
        writer.WriteSingle(velocityY);
        writer.WriteInt16(stack);
        writer.WriteByte(prefix);
        writer.WriteByte(stateFlags);
        writer.WriteInt16(itemType);
    }

    public static (
        short ItemIndex,
        float PositionX,
        float PositionY,
        float VelocityX,
        float VelocityY,
        short Stack,
        byte Prefix,
        byte StateFlags,
        short ItemType) ReadFields(PacketWireReader reader)
    {
        short index = reader.ReadInt16();
        float positionX = reader.ReadSingle();
        float positionY = reader.ReadSingle();
        float velocityX = reader.ReadSingle();
        float velocityY = reader.ReadSingle();
        short stack = reader.ReadInt16();
        byte prefix = reader.ReadByte();
        byte stateFlags = reader.ReadByte();
        short itemType = reader.ReadInt16();
        return (ItemIndex: index, PositionX: positionX, PositionY: positionY, VelocityX: velocityX, VelocityY: velocityY, Stack: stack, Prefix: prefix, StateFlags: stateFlags, ItemType: itemType);
    }
}
