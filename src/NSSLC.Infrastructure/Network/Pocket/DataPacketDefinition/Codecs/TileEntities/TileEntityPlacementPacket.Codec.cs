using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileEntityPlacementPacket
{
    public static void Write(PacketWireWriter writer, short x, short y, byte entityType)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteByte(entityType);
    }

    public static (short X, short Y, byte EntityType) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16(), EntityType: reader.ReadByte());
}
