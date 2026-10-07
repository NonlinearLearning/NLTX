using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncTilePickingPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        short x,
        short y,
        byte tileType)
    {
        writer.WriteByte(player);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteByte(tileType);
    }

    public static (byte Player, short X, short Y, byte TileType) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        byte tileType = reader.ReadByte();
        return (Player: player, X: x, Y: y, TileType: tileType);
    }
}
