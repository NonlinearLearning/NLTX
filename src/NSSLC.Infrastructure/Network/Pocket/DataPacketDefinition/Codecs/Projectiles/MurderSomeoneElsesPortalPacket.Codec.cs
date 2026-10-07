using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MurderSomeoneElsesPortalPacket
{
    public static void Write(PacketWireWriter writer, ushort projectileOwner, byte portalIndex)
    {
        writer.WriteUInt16(projectileOwner);
        writer.WriteByte(portalIndex);
    }

    public static (ushort ProjectileOwner, byte PortalIndex) Read(PacketWireReader reader) => (ProjectileOwner: reader.ReadUInt16(), PortalIndex: reader.ReadByte());
}
