using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerChestPacket
{
    public static void Write(
        PacketWireWriter writer,
        short chestIndex,
        short chestX,
        short chestY,
        byte nameLengthIndicator,
        string? name)
    {
        bool carriesName = nameLengthIndicator is >= 1 and <= 20;
        if (carriesName != (name is not null))
            throw new PacketWireFormatException("Packet 33 name presence must match its 1..20 name-length indicator.");
        writer.WriteInt16(chestIndex);
        writer.WriteInt16(chestX);
        writer.WriteInt16(chestY);
        writer.WriteByte(nameLengthIndicator);
        if (carriesName)
            writer.WriteString(name!);
    }

    public static (
        short ChestIndex,
        short ChestX,
        short ChestY,
        byte NameLengthIndicator,
        string? Name) Read(PacketWireReader reader)
    {
        short chest = reader.ReadInt16();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        byte nameLength = reader.ReadByte();
        string? name = nameLength is >= 1 and <= 20 ? reader.ReadString() : null;
        return (ChestIndex: chest, ChestX: x, ChestY: y, NameLengthIndicator: nameLength, Name: name);
    }
}
