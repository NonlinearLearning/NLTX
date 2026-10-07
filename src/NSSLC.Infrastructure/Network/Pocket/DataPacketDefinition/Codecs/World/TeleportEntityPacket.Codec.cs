using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportEntityPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte entityKind,
        short entityIndex,
        PacketVector2 position,
        byte style,
        bool useCurrentEntityPosition,
        int? teleportValue)
    {
        if (entityKind > 3)
            throw new PacketWireFormatException("Packet 65 entity kind must fit in the first two flag bits.");
        if (teleportValue == 0)
            throw new PacketWireFormatException("Packet 65 encodes the optional teleport value only when it is nonzero.");
        byte flags = entityKind;
        if (useCurrentEntityPosition)
            flags |= 1 << 2;
        if (teleportValue.HasValue)
            flags |= 1 << 3;
        writer.WriteByte(flags);
        writer.WriteInt16(entityIndex);
        writer.WriteSingle(position.X);
        writer.WriteSingle(position.Y);
        writer.WriteByte(style);
        if (teleportValue is int value)
            writer.WriteInt32(value);
    }

    public static (
        byte EntityKind,
        short EntityIndex,
        PacketVector2 Position,
        byte Style,
        bool UseCurrentEntityPosition,
        int? TeleportValue) Read(PacketWireReader reader)
    {
        byte flags = reader.ReadByte();
        short entityIndex = reader.ReadInt16();
        var position = new PacketVector2(reader.ReadSingle(), reader.ReadSingle());
        byte style = reader.ReadByte();
        int? teleportValue = (flags & (1 << 3)) != 0 ? reader.ReadInt32() : null;
        if (teleportValue == 0)
            throw new PacketWireFormatException("Packet 65 reference writers do not set the extra-value flag for zero.");
        var payload = (EntityKind: (byte)(flags & 3), EntityIndex: entityIndex, Position: position, Style: style, UseCurrentEntityPosition: (flags & (1 << 2)) != 0, TeleportValue: teleportValue);
        return payload;
    }
}
