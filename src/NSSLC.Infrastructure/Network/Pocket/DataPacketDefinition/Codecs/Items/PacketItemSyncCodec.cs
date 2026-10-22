using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class PacketItemSyncCodec
{
    // Current 21/90 share the base fields with the historical 145/148 variants.
    // Keep the base codec separate so those variants cannot consume their tails twice.
    public static void WriteCurrentFields(
        PacketWireWriter writer, short itemIndex, float positionX, float positionY,
        float velocityX, float velocityY, short stack, byte prefix, byte stateFlags,
        short itemType, bool? shimmered, float? shimmerTime, byte? enemyGrabDelayTime)
    {
        ValidateCurrent(itemIndex, positionX, positionY, velocityX, velocityY, stack,
            stateFlags, itemType, shimmerTime);
        if (((stateFlags & 4) != 0) != (shimmered.HasValue && shimmerTime.HasValue) ||
            shimmered.HasValue != shimmerTime.HasValue ||
            ((stateFlags & 8) != 0) != enemyGrabDelayTime.HasValue)
        {
            throw new PacketWireFormatException("Item flags do not match their optional fields.");
        }
        WriteFields(writer, itemIndex, positionX, positionY, velocityX, velocityY,
            stack, prefix, stateFlags, itemType);
        if (shimmered.HasValue)
        {
            writer.WriteBoolean(shimmered.Value);
            writer.WriteSingle(shimmerTime!.Value);
        }
        if (enemyGrabDelayTime.HasValue)
        {
            writer.WriteByte(enemyGrabDelayTime.Value);
        }
    }

    public static (short ItemIndex, float PositionX, float PositionY, float VelocityX,
        float VelocityY, short Stack, byte Prefix, byte StateFlags, short ItemType,
        bool? Shimmered, float? ShimmerTime, byte? EnemyGrabDelayTime)
        ReadCurrentFields(PacketWireReader reader)
    {
        var body = ReadFields(reader);
        bool? shimmered = null;
        float? shimmerTime = null;
        byte? enemyGrabDelayTime = null;
        if ((body.StateFlags & 4) != 0)
        {
            byte value = reader.ReadByte();
            if (value > 1)
            {
                throw new PacketWireFormatException("Shimmered must be zero or one.");
            }
            shimmered = value != 0;
            shimmerTime = reader.ReadSingle();
        }
        if ((body.StateFlags & 8) != 0)
        {
            enemyGrabDelayTime = reader.ReadByte();
        }
        ValidateCurrent(body.ItemIndex, body.PositionX, body.PositionY, body.VelocityX,
            body.VelocityY, body.Stack, body.StateFlags, body.ItemType, shimmerTime);
        return (body.ItemIndex, body.PositionX, body.PositionY, body.VelocityX, body.VelocityY,
            body.Stack, body.Prefix, body.StateFlags, body.ItemType, shimmered, shimmerTime,
            enemyGrabDelayTime);
    }

    private static void ValidateCurrent(short index, float x, float y, float vx, float vy,
        short stack, byte flags, short type, float? shimmerTime)
    {
        if (index is < 0 or > 400 || stack < 0 || type < 0 || (flags & 0xf0) != 0 ||
            !float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(vx) ||
            !float.IsFinite(vy) || shimmerTime.HasValue && !float.IsFinite(shimmerTime.Value))
        {
            throw new PacketWireFormatException("Item sync contains invalid fields.");
        }
    }

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
