namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldTileHeaderExtensionValue
{
  public WorldTileHeaderExtensionValue(
    bool actuator,
    bool inactive,
    bool wire4,
    byte tileColor,
    byte wallColor,
    bool invisibleBlock,
    bool invisibleWall,
    bool fullbrightBlock,
    bool fullbrightWall,
    bool shimmer,
    byte reservedHeader4Bits)
  {
    if ((reservedHeader4Bits & 0x1F) != 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(reservedHeader4Bits),
        "Only the reserved Header4 high bits may be preserved.");
    }

    Actuator = actuator;
    Inactive = inactive;
    Wire4 = wire4;
    TileColor = tileColor;
    WallColor = wallColor;
    InvisibleBlock = invisibleBlock;
    InvisibleWall = invisibleWall;
    FullbrightBlock = fullbrightBlock;
    FullbrightWall = fullbrightWall;
    Shimmer = shimmer;
    ReservedHeader4Bits = reservedHeader4Bits;
  }

  public bool Actuator { get; }

  public bool Inactive { get; }

  public bool Wire4 { get; }

  public byte TileColor { get; }

  public byte WallColor { get; }

  public bool InvisibleBlock { get; }

  public bool InvisibleWall { get; }

  public bool FullbrightBlock { get; }

  public bool FullbrightWall { get; }

  public bool Shimmer { get; }

  public byte ReservedHeader4Bits { get; }

  internal bool HasHeader3 =>
    Actuator || Inactive || Wire4 || TileColor != 0 || WallColor != 0 ||
    InvisibleBlock || InvisibleWall || FullbrightBlock || FullbrightWall || Shimmer || HasHeader4;

  internal bool HasHeader4 =>
    InvisibleBlock || InvisibleWall || FullbrightBlock || FullbrightWall || ReservedHeader4Bits != 0;
}
