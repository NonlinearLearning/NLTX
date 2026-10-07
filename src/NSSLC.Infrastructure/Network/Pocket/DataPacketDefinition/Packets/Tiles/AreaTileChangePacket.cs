namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AreaTileChangePacket
{
    public short StartX { get; set; }
    public short StartY { get; set; }
    public byte Width { get; set; }
    public byte Height { get; set; }
    public byte ChangeType { get; set; }
    // Flattened in wire order: x is the outer loop and y is the inner loop.
    public Packet20Tile[] Tiles { get; set; } = [];
}

public struct Packet20Tile
{
    public bool Active { get; set; }
    public ushort TileType { get; set; }
    public short FrameX { get; set; }
    public short FrameY { get; set; }
    public byte TileColor { get; set; }
    public ushort Wall { get; set; }
    public byte WallColor { get; set; }
    public byte LiquidAmount { get; set; }
    public byte LiquidType { get; set; }
    public bool Wire { get; set; }
    public bool HalfBrick { get; set; }
    public bool Actuator { get; set; }
    public bool Inactive { get; set; }
    public bool Wire2 { get; set; }
    public bool Wire3 { get; set; }
    public byte Slope { get; set; }
    public bool Wire4 { get; set; }
    public bool FullbrightBlock { get; set; }
    public bool FullbrightWall { get; set; }
    public bool InvisibleBlock { get; set; }
    public bool InvisibleWall { get; set; }
}
