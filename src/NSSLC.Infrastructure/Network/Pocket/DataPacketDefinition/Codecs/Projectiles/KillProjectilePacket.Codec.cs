using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class KillProjectilePacket
{
    public static void Write(PacketWireWriter writer, short projectileIdentity, byte owner)
    {
        writer.WriteInt16(projectileIdentity);
        writer.WriteByte(owner);
    }

    public static (short ProjectileIdentity, byte Owner) Read(PacketWireReader reader) => (ProjectileIdentity: reader.ReadInt16(), Owner: reader.ReadByte());
}
