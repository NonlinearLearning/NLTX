using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class WiredCannonShotPacket
{
    public static void Write(
        PacketWireWriter writer,
        short damage,
        float knockback,
        short x,
        short y,
        short angle,
        short ammo,
        byte owner)
    {
        writer.WriteInt16(damage);
        writer.WriteSingle(knockback);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteInt16(angle);
        writer.WriteInt16(ammo);
        writer.WriteByte(owner);
    }

    public static (
        short Damage,
        float Knockback,
        short X,
        short Y,
        short Angle,
        short Ammo,
        byte Owner) Read(PacketWireReader reader) => (Damage: reader.ReadInt16(), Knockback: reader.ReadSingle(), X: reader.ReadInt16(), Y: reader.ReadInt16(), Angle: reader.ReadInt16(), Ammo: reader.ReadInt16(), Owner: reader.ReadByte());
}
