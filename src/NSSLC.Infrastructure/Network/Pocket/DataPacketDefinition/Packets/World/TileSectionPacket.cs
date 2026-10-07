namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileSectionPacket
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public short Width { get; set; }
    public short Height { get; set; }
    // Tiles follow wire order: y outer, x inner.
    public Packet10Tile[] Tiles { get; set; } = [];
    public Packet10ChestRecord[] Chests { get; set; } = [];
    public Packet10SignRecord[] Signs { get; set; } = [];
    public PacketTileEntityRecord[] TileEntities { get; set; } = [];
}

public readonly record struct Packet10Tile
{
    public bool Active { get; init; }
    public ushort Type { get; init; }
    public short FrameX { get; init; }
    public short FrameY { get; init; }
    public byte TileColor { get; init; }
    public ushort Wall { get; init; }
    public byte WallColor { get; init; }
    public byte LiquidAmount { get; init; }
    public Packet10LiquidType LiquidType { get; init; }
    public bool Wire { get; init; }
    public bool Wire2 { get; init; }
    public bool Wire3 { get; init; }
    public bool Wire4 { get; init; }
    public bool HalfBrick { get; init; }
    public byte Slope { get; init; }
    public bool Actuator { get; init; }
    public bool Inactive { get; init; }
    public bool InvisibleBlock { get; init; }
    public bool InvisibleWall { get; init; }
    public bool FullbrightBlock { get; init; }
    public bool FullbrightWall { get; init; }
}

public enum Packet10LiquidType : byte
{
    Water,
    Lava,
    Honey,
    Shimmer
}

public readonly record struct Packet10ChestRecord(short Id, short X, short Y, string Name);

public readonly record struct Packet10SignRecord(short Id, short X, short Y, string Text);
