using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class PlayerDeathReasonCodec
{
    public static void Write(PacketWireWriter writer, PlayerDeathReason reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        byte flags = 0;
        Set(ref flags, 0, reason.SourcePlayerIndex is not null and not -1);
        Set(ref flags, 1, reason.SourceNpcIndex is not null and not -1);
        Set(ref flags, 2, reason.SourceProjectileIndex is not null and not -1);
        Set(ref flags, 3, reason.SourceOtherIndex is not null);
        Set(ref flags, 4, reason.SourceProjectileType is not null and not 0);
        Set(ref flags, 5, reason.SourceItemType is not null and not 0);
        Set(ref flags, 6, reason.SourceItemPrefix is not null and not 0);
        Set(ref flags, 7, reason.CustomReason is not null);
        writer.WriteByte(flags);
        if ((flags & 1) != 0) writer.WriteInt16(reason.SourcePlayerIndex!.Value);
        if ((flags & 2) != 0) writer.WriteInt16(reason.SourceNpcIndex!.Value);
        if ((flags & 4) != 0) writer.WriteInt16(reason.SourceProjectileIndex!.Value);
        if ((flags & 8) != 0) writer.WriteByte(reason.SourceOtherIndex!.Value);
        if ((flags & 16) != 0) writer.WriteInt16(reason.SourceProjectileType!.Value);
        if ((flags & 32) != 0) writer.WriteInt16(reason.SourceItemType!.Value);
        if ((flags & 64) != 0) writer.WriteByte(reason.SourceItemPrefix!.Value);
        if ((flags & 128) != 0) writer.WriteString(reason.CustomReason!);
    }

    public static PlayerDeathReason Read(PacketWireReader reader)
    {
        byte flags = reader.ReadByte();
        return new PlayerDeathReason
        {
            SourcePlayerIndex = Has(flags, 0) ? reader.ReadInt16() : null,
            SourceNpcIndex = Has(flags, 1) ? reader.ReadInt16() : null,
            SourceProjectileIndex = Has(flags, 2) ? reader.ReadInt16() : null,
            SourceOtherIndex = Has(flags, 3) ? reader.ReadByte() : null,
            SourceProjectileType = Has(flags, 4) ? reader.ReadInt16() : null,
            SourceItemType = Has(flags, 5) ? reader.ReadInt16() : null,
            SourceItemPrefix = Has(flags, 6) ? reader.ReadByte() : null,
            CustomReason = Has(flags, 7) ? reader.ReadString() : null
        };
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;

    private static void Set(ref byte flags, int bit, bool set)
    {
        if (set) flags |= (byte)(1 << bit);
    }
}
