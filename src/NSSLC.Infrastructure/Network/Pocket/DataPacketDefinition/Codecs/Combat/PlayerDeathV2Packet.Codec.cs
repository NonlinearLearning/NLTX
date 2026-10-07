using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerDeathV2Packet
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        PlayerDeathReason reason,
        short damage,
        byte directionCode,
        byte deathFlags)
    {
        writer.WriteByte(player);
        PlayerDeathReasonCodec.Write(writer, reason);
        writer.WriteInt16(damage);
        writer.WriteByte(directionCode);
        writer.WriteByte(deathFlags);
    }

    public static (
        byte Player,
        PlayerDeathReason Reason,
        short Damage,
        byte DirectionCode,
        byte DeathFlags) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        PlayerDeathReason reason = PlayerDeathReasonCodec.Read(reader);
        short damage = reader.ReadInt16();
        byte direction = reader.ReadByte();
        byte flags = reader.ReadByte();
        return (Player: player, Reason: reason, Damage: damage, DirectionCode: direction, DeathFlags: flags);
    }
}
