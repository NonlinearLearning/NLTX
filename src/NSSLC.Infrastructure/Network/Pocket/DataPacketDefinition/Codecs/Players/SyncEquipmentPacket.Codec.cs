using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncEquipmentPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        short slot,
        short stack,
        byte prefix,
        short itemType,
        bool favorited,
        bool isCreativeItem)
    {
        writer.WriteByte(player);
        writer.WriteInt16(slot);
        writer.WriteInt16(stack);
        writer.WriteByte(prefix);
        writer.WriteInt16(itemType);
        writer.WriteByte(Flags(favorited, isCreativeItem));
    }

    public static (
        byte Player,
        short Slot,
        short Stack,
        byte Prefix,
        short ItemType,
        bool Favorited,
        bool IsCreativeItem) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        short slot = reader.ReadInt16();
        short stack = reader.ReadInt16();
        byte prefix = reader.ReadByte();
        short itemType = reader.ReadInt16();
        byte flags = reader.ReadByte();
        return (Player: player, Slot: slot, Stack: stack, Prefix: prefix, ItemType: itemType, Favorited: Has(flags, 0), IsCreativeItem: Has(flags, 1));
    }

    private static byte Flags(bool favorited, bool creative)
    {
        byte flags = 0;
        if (favorited)
            flags |= 1;
        if (creative)
            flags |= 2;
        return flags;
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
}
