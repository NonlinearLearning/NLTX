using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemTweakerPacket
{
    private const short MaximumItemSlots = 400;
    private const byte KnownExtraFlags = 0x3F;

    public static void Write(
        PacketWireWriter writer,
        short itemIndex,
        byte flags,
        uint? color,
        ushort? damage,
        float? knockback,
        ushort? useAnimation,
        ushort? useTime,
        short? shoot,
        float? shootSpeed,
        Packet88ExtraValues? extra)
    {
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots)
            throw new PacketWireFormatException("Packet 88 item slot is outside the Steam world-item range.");

        RequirePresence(flags, 0, color is not null, "color");
        RequirePresence(flags, 1, damage is not null, "damage");
        RequirePresence(flags, 2, knockback is not null, "knockback");
        RequirePresence(flags, 3, useAnimation is not null, "use animation");
        RequirePresence(flags, 4, useTime is not null, "use time");
        RequirePresence(flags, 5, shoot is not null, "shoot type");
        RequirePresence(flags, 6, shootSpeed is not null, "shoot speed");
        RequirePresence(flags, 7, extra is not null, "extended item fields");
        if ((knockback is float knockbackValue && !float.IsFinite(knockbackValue)) ||
            (shootSpeed is float shootSpeedValue && !float.IsFinite(shootSpeedValue)))
            throw new PacketWireFormatException("Packet 88 contains a non-finite item value.");
        if (extra is Packet88ExtraValues extraValue)
        {
            if ((extraValue.Flags & ~KnownExtraFlags) != 0 ||
                (extraValue.Scale is float scaleValue && !float.IsFinite(scaleValue)))
                throw new PacketWireFormatException("Packet 88 contains unsupported extra flags or a non-finite scale.");

            RequirePresence(extraValue.Flags, 0, extraValue.Width is not null, "width");
            RequirePresence(extraValue.Flags, 1, extraValue.Height is not null, "height");
            RequirePresence(extraValue.Flags, 2, extraValue.Scale is not null, "scale");
            RequirePresence(extraValue.Flags, 3, extraValue.Ammo is not null, "ammo");
            RequirePresence(extraValue.Flags, 4, extraValue.UseAmmo is not null, "use ammo");
            RequirePresence(extraValue.Flags, 5, extraValue.NotAmmo is not null, "not-ammo flag");
        }

        writer.WriteInt16(itemIndex);
        writer.WriteByte(flags);
        if (Has(flags, 0))
            writer.WriteUInt32(color!.Value);
        if (Has(flags, 1))
            writer.WriteUInt16(damage!.Value);
        if (Has(flags, 2))
            writer.WriteSingle(knockback!.Value);
        if (Has(flags, 3))
            writer.WriteUInt16(useAnimation!.Value);
        if (Has(flags, 4))
            writer.WriteUInt16(useTime!.Value);
        if (Has(flags, 5))
            writer.WriteInt16(shoot!.Value);
        if (Has(flags, 6))
            writer.WriteSingle(shootSpeed!.Value);
        if (extra is Packet88ExtraValues additional)
        {
            writer.WriteByte(additional.Flags);
            if (Has(additional.Flags, 0))
                writer.WriteUInt16(additional.Width!.Value);
            if (Has(additional.Flags, 1))
                writer.WriteUInt16(additional.Height!.Value);
            if (Has(additional.Flags, 2))
                writer.WriteSingle(additional.Scale!.Value);
            if (Has(additional.Flags, 3))
                writer.WriteInt16(additional.Ammo!.Value);
            if (Has(additional.Flags, 4))
                writer.WriteInt16(additional.UseAmmo!.Value);
            if (Has(additional.Flags, 5))
                writer.WriteBoolean(additional.NotAmmo!.Value);
        }
    }

    public static (
        short ItemIndex,
        byte Flags,
        uint? Color,
        ushort? Damage,
        float? Knockback,
        ushort? UseAnimation,
        ushort? UseTime,
        short? Shoot,
        float? ShootSpeed,
        Packet88ExtraValues? Extra) Read(PacketWireReader reader)
    {
        short itemIndex = reader.ReadInt16();
        if (itemIndex < 0 || itemIndex >= MaximumItemSlots)
            throw new PacketWireFormatException("Packet 88 item slot is outside the Steam world-item range.");

        byte flags = reader.ReadByte();
        uint? color = Has(flags, 0) ? reader.ReadUInt32() : null;
        ushort? damage = Has(flags, 1) ? reader.ReadUInt16() : null;
        float? knockback = Has(flags, 2) ? reader.ReadSingle() : null;
        ushort? useAnimation = Has(flags, 3) ? reader.ReadUInt16() : null;
        ushort? useTime = Has(flags, 4) ? reader.ReadUInt16() : null;
        short? shoot = Has(flags, 5) ? reader.ReadInt16() : null;
        float? shootSpeed = Has(flags, 6) ? reader.ReadSingle() : null;
        if ((knockback is float knockbackValue && !float.IsFinite(knockbackValue)) ||
            (shootSpeed is float shootSpeedValue && !float.IsFinite(shootSpeedValue)))
            throw new PacketWireFormatException("Packet 88 contains a non-finite item value.");
        Packet88ExtraValues? extra = null;
        if (Has(flags, 7))
        {
            byte extraFlags = reader.ReadByte();
            if ((extraFlags & ~KnownExtraFlags) != 0)
                throw new PacketWireFormatException("Packet 88 contains unsupported extra flags.");

            ushort? width = Has(extraFlags, 0) ? reader.ReadUInt16() : null;
            ushort? height = Has(extraFlags, 1) ? reader.ReadUInt16() : null;
            float? scale = Has(extraFlags, 2) ? reader.ReadSingle() : null;
            if (scale is float scaleValue && !float.IsFinite(scaleValue))
                throw new PacketWireFormatException("Packet 88 contains a non-finite scale.");
            short? ammo = Has(extraFlags, 3) ? reader.ReadInt16() : null;
            short? useAmmo = Has(extraFlags, 4) ? reader.ReadInt16() : null;
            bool? notAmmo = Has(extraFlags, 5) ? reader.ReadBoolean() : null;
            extra = new Packet88ExtraValues(extraFlags, width, height, scale, ammo, useAmmo, notAmmo);
        }

        return (ItemIndex: itemIndex, Flags: flags, Color: color, Damage: damage, Knockback: knockback, UseAnimation: useAnimation, UseTime: useTime, Shoot: shoot, ShootSpeed: shootSpeed, Extra: extra);
    }

    private static void RequirePresence(byte flags, int bit, bool present, string field)
    {
        if (Has(flags, bit) != present)
            throw new PacketWireFormatException($"Packet 88 presence flag for {field} does not match its value.");
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
}
