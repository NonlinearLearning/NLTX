using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class KillProjectilePacket
{
    public static void Write(
        PacketWireWriter writer,
        short projectileIdentity,
        byte owner,
        PacketVector2 position)
    {
        uint key = (uint)(owner | ((projectileIdentity & 0x3ff) << 8));
        writer.WriteUInt32(key);
        writer.WriteSingle(position.X);
        writer.WriteSingle(position.Y);
    }

    public static (short ProjectileIdentity, byte Owner, PacketVector2 Position) Read(PacketWireReader reader)
    {
        uint key = reader.ReadUInt32();
        return (
            ProjectileIdentity: checked((short)((key >> 8) & 0x3ff)),
            Owner: (byte)(key & 0xff),
            Position: new PacketVector2(reader.ReadSingle(), reader.ReadSingle()));
    }
}
