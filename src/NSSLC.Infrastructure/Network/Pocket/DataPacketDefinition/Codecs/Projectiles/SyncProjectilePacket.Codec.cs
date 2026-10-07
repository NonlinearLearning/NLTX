using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncProjectilePacket
{
    public static void Write(
        PacketWireWriter writer,
        short identity,
        PacketVector2 position,
        PacketVector2 velocity,
        byte owner,
        short projectileType,
        float ai0,
        float ai1,
        ushort? bannerId,
        short? damage,
        float? knockback,
        short? originalDamage,
        short? projectileUuid,
        float ai2,
        Func<short, bool> needsUuid)
    {
        ArgumentNullException.ThrowIfNull(needsUuid);
        bool hasUuid = needsUuid(projectileType);
        if (hasUuid != projectileUuid.HasValue)
            throw new PacketWireFormatException("Packet 27 projectile UUID presence must match the projectile type protocol fact.");
        byte flags1 = 0;
        Set(ref flags1, 0, ai0 != 0f);
        Set(ref flags1, 1, ai1 != 0f);
        Set(ref flags1, 2, ai2 != 0f);
        Set(ref flags1, 3, bannerId is not null and not 0);
        Set(ref flags1, 4, damage is not null and not 0);
        Set(ref flags1, 5, knockback is not null and not 0f);
        Set(ref flags1, 6, originalDamage is not null and not 0);
        Set(ref flags1, 7, hasUuid);
        byte flags2 = 0;
        Set(ref flags2, 0, ai2 != 0f);
        writer.WriteInt16(identity);
        WriteVector2(writer, position);
        WriteVector2(writer, velocity);
        writer.WriteByte(owner);
        writer.WriteInt16(projectileType);
        writer.WriteByte(flags1);
        if ((flags1 & (1 << 2)) != 0)
            writer.WriteByte(flags2);
        if (Has(flags1, 0))
            writer.WriteSingle(ai0);
        if (Has(flags1, 1))
            writer.WriteSingle(ai1);
        if (Has(flags1, 3))
            writer.WriteUInt16(bannerId!.Value);
        if (Has(flags1, 4))
            writer.WriteInt16(damage!.Value);
        if (Has(flags1, 5))
            writer.WriteSingle(knockback!.Value);
        if (Has(flags1, 6))
            writer.WriteInt16(originalDamage!.Value);
        if (Has(flags1, 7))
            writer.WriteInt16(projectileUuid!.Value);
        if ((flags2 & 1) != 0)
            writer.WriteSingle(ai2);
    }

    public static (
        short Identity,
        PacketVector2 Position,
        PacketVector2 Velocity,
        byte Owner,
        short ProjectileType,
        float Ai0,
        float Ai1,
        ushort? BannerId,
        short? Damage,
        float? Knockback,
        short? OriginalDamage,
        short? ProjectileUuid,
        float Ai2) Read(PacketWireReader reader)
    {
        short identity = reader.ReadInt16();
        PacketVector2 position = ReadVector2(reader);
        PacketVector2 velocity = ReadVector2(reader);
        byte owner = reader.ReadByte();
        short type = reader.ReadInt16();
        byte flags1 = reader.ReadByte();
        byte flags2 = Has(flags1, 2) ? reader.ReadByte() : (byte)0;
        float ai0 = Has(flags1, 0) ? reader.ReadSingle() : 0f;
        float ai1 = Has(flags1, 1) ? reader.ReadSingle() : 0f;
        ushort? bannerId = Has(flags1, 3) ? reader.ReadUInt16() : null;
        short? damage = Has(flags1, 4) ? reader.ReadInt16() : null;
        float? knockback = Has(flags1, 5) ? reader.ReadSingle() : null;
        short? originalDamage = Has(flags1, 6) ? reader.ReadInt16() : null;
        short? uuid = Has(flags1, 7) ? reader.ReadInt16() : null;
        float ai2 = Has(flags2, 0) ? reader.ReadSingle() : 0f;
        return (Identity: identity, Position: position, Velocity: velocity, Owner: owner, ProjectileType: type, Ai0: ai0, Ai1: ai1, BannerId: bannerId, Damage: damage, Knockback: knockback, OriginalDamage: originalDamage, ProjectileUuid: uuid, Ai2: ai2);
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
    private static void Set(ref byte flags, int bit, bool value)
    {
        if (value)
            flags |= (byte)(1 << bit);
    }

    private static void WriteVector2(PacketWireWriter writer, PacketVector2 value)
    {
        writer.WriteSingle(value.X);
        writer.WriteSingle(value.Y);
    }

    private static PacketVector2 ReadVector2(PacketWireReader reader) => new(reader.ReadSingle(), reader.ReadSingle());
}
