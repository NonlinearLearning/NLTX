using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AreaTileChangePacket
{
    public static void WriteTileBytes(
        PacketWireWriter writer,
        byte width,
        byte height,
        bool[] frameImportant,
        bool isServer,
        Packet20Tile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(frameImportant);
        int tileCount = width * height;
        RequireTileCount(tileCount, tiles);
        for (int index = 0; index < tileCount; index++)
            WriteTile(writer, tiles[index], frameImportant, isServer);
    }

    private static void WriteTile(PacketWireWriter writer, Packet20Tile tile, bool[] frameImportant, bool isServer)
    {
        if (tile.Slope > 7)
        {
            throw new PacketWireFormatException($"Packet 20 slope {tile.Slope} does not fit in three bits.");
        }

        bool hasLiquid = tile.LiquidAmount > 0;
        byte flags1 = 0;
        Set(ref flags1, 0, tile.Active);
        Set(ref flags1, 2, tile.Wall > 0);
        Set(ref flags1, 3, hasLiquid && isServer);
        Set(ref flags1, 4, tile.Wire);
        Set(ref flags1, 5, tile.HalfBrick);
        Set(ref flags1, 6, tile.Actuator);
        Set(ref flags1, 7, tile.Inactive);
        byte flags2 = (byte)(tile.Slope << 4);
        Set(ref flags2, 0, tile.Wire2);
        Set(ref flags2, 1, tile.Wire3);
        Set(ref flags2, 2, tile.Active && tile.TileColor > 0);
        Set(ref flags2, 3, tile.Wall > 0 && tile.WallColor > 0);
        Set(ref flags2, 7, tile.Wire4);
        byte flags3 = 0;
        Set(ref flags3, 0, tile.FullbrightBlock);
        Set(ref flags3, 1, tile.FullbrightWall);
        Set(ref flags3, 2, tile.InvisibleBlock);
        Set(ref flags3, 3, tile.InvisibleWall);
        writer.WriteByte(flags1);
        writer.WriteByte(flags2);
        writer.WriteByte(flags3);
        if ((flags2 & (1 << 2)) != 0)
            writer.WriteByte(tile.TileColor);
        if ((flags2 & (1 << 3)) != 0)
            writer.WriteByte(tile.WallColor);
        if (tile.Active)
        {
            writer.WriteUInt16(tile.TileType);
            if (IsFrameImportant(frameImportant, tile.TileType))
            {
                writer.WriteInt16(tile.FrameX);
                writer.WriteInt16(tile.FrameY);
            }
        }

        if (tile.Wall > 0)
            writer.WriteUInt16(tile.Wall);
        // The reference sender gates the flag on server mode, but emits liquid
        // payload bytes whenever the amount is nonzero.
        if (hasLiquid)
        {
            writer.WriteByte(tile.LiquidAmount);
            writer.WriteByte(tile.LiquidType);
        }
    }

    public static Packet20Tile[] ReadTileBytes(PacketWireReader reader, byte width, byte height, bool[] frameImportant)
    {
        ArgumentNullException.ThrowIfNull(frameImportant);
        int tileCount = width * height;
        var tiles = new Packet20Tile[tileCount];
        for (int index = 0; index < tileCount; index++)
        {
            byte flags1 = reader.ReadByte();
            byte flags2 = reader.ReadByte();
            byte flags3 = reader.ReadByte();
            var tile = new Packet20Tile
            {
                Active = Has(flags1, 0),
                Wire = Has(flags1, 4),
                HalfBrick = Has(flags1, 5),
                Actuator = Has(flags1, 6),
                Inactive = Has(flags1, 7),
                Wire2 = Has(flags2, 0),
                Wire3 = Has(flags2, 1),
                Slope = (byte)((flags2 >> 4) & 0x07),
                Wire4 = Has(flags2, 7),
                FullbrightBlock = Has(flags3, 0),
                FullbrightWall = Has(flags3, 1),
                InvisibleBlock = Has(flags3, 2),
                InvisibleWall = Has(flags3, 3)
            };
            if (Has(flags2, 2))
                tile.TileColor = reader.ReadByte();
            if (Has(flags2, 3))
                tile.WallColor = reader.ReadByte();
            if (tile.Active)
            {
                tile.TileType = reader.ReadUInt16();
                if (IsFrameImportant(frameImportant, tile.TileType))
                {
                    tile.FrameX = reader.ReadInt16();
                    tile.FrameY = reader.ReadInt16();
                }
            }

            if (Has(flags1, 2))
                tile.Wall = reader.ReadUInt16();
            if (Has(flags1, 3))
            {
                tile.LiquidAmount = reader.ReadByte();
                tile.LiquidType = reader.ReadByte();
            }

            tiles[index] = tile;
        }

        return tiles;
    }

    private static bool IsFrameImportant(bool[] frameImportant, ushort tileType)
    {
        if (tileType >= frameImportant.Length)
            throw new PacketWireFormatException($"Tile type {tileType} is outside the supplied frame-important table.");
        return frameImportant[tileType];
    }

    private static void RequireTileCount(int expected, Packet20Tile[] tiles)
    {
        if (tiles is null || tiles.Length != expected)
        {
            throw new PacketWireFormatException($"Packet 20 must contain exactly {expected} tile value(s).");
        }
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
    private static void Set(ref byte flags, int bit, bool value)
    {
        if (value)
            flags |= (byte)(1 << bit);
    }
}
