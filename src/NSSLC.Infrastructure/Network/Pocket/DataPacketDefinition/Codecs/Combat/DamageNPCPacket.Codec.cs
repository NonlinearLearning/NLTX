using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class DamageNPCPacket
{
    public static void Write(
        PacketWireWriter writer,
        short npcIndex,
        short damage,
        float knockback,
        byte encodedDirection,
        byte hitDirection)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteInt16(damage);
        writer.WriteSingle(knockback);
        writer.WriteByte(encodedDirection);
        writer.WriteByte(hitDirection);
    }

    public static (
        short NpcIndex,
        short Damage,
        float Knockback,
        byte EncodedDirection,
        byte HitDirection) Read(PacketWireReader reader)
    {
        short npc = reader.ReadInt16();
        short damage = reader.ReadInt16();
        float knockback = reader.ReadSingle();
        byte direction = reader.ReadByte();
        byte hitDirection = reader.ReadByte();
        return (NpcIndex: npc, Damage: damage, Knockback: knockback, EncodedDirection: direction, HitDirection: hitDirection);
    }
}
