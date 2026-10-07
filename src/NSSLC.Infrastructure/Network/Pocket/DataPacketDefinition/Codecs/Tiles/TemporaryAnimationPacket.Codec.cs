using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TemporaryAnimationPacket
{
    public static void Write(
        PacketWireWriter writer,
        short animationType,
        ushort tileType,
        short x,
        short y)
    {
        writer.WriteInt16(animationType);
        writer.WriteUInt16(tileType);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
    }

    public static (short AnimationType, ushort TileType, short X, short Y) Read(PacketWireReader reader) => (AnimationType: reader.ReadInt16(), TileType: reader.ReadUInt16(), X: reader.ReadInt16(), Y: reader.ReadInt16());
}
