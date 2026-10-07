using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerHurtV2Packet
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        PlayerDeathReason reason,
        short damage,
        byte directionCode,
        byte hitFlags,
        sbyte cooldownCounter)
    {
        writer.WriteByte(player);
        PlayerDeathReasonCodec.Write(writer, reason);
        writer.WriteInt16(damage);
        writer.WriteByte(directionCode);
        writer.WriteByte(hitFlags);
        writer.WriteSByte(cooldownCounter);
    }

    public static (
        byte Player,
        PlayerDeathReason Reason,
        short Damage,
        byte DirectionCode,
        byte HitFlags,
        sbyte CooldownCounter) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        PlayerDeathReason reason = PlayerDeathReasonCodec.Read(reader);
        short damage = reader.ReadInt16();
        byte direction = reader.ReadByte();
        byte flags = reader.ReadByte();
        sbyte cooldown = reader.ReadSByte();
        return (Player: player, Reason: reason, Damage: damage, DirectionCode: direction, HitFlags: flags, CooldownCounter: cooldown);
    }
}
