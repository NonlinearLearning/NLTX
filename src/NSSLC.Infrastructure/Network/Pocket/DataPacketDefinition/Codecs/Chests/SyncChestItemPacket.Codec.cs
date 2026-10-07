using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncChestItemPacket
{
    public static void Write(
        PacketWireWriter writer,
        short chestIndex,
        byte slot,
        short stack,
        byte prefix,
        short itemType)
    {
        writer.WriteInt16(chestIndex);
        writer.WriteByte(slot);
        writer.WriteInt16(stack);
        writer.WriteByte(prefix);
        writer.WriteInt16(itemType);
    }

    public static (
        short ChestIndex,
        byte Slot,
        short Stack,
        byte Prefix,
        short ItemType) Read(PacketWireReader reader)
    {
        short chest = reader.ReadInt16();
        byte slot = reader.ReadByte();
        short stack = reader.ReadInt16();
        byte prefix = reader.ReadByte();
        short type = reader.ReadInt16();
        return (ChestIndex: chest, Slot: slot, Stack: stack, Prefix: prefix, ItemType: type);
    }
}
