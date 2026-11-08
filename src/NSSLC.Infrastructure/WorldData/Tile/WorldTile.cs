namespace Terraria.WorldData;

public enum LiquidType : byte
{
  Water = 0,
  Lava = 1,
  Honey = 2,
  Shimmer = 3
}

[Flags]
public enum TileWireMask : byte
{
  None = 0,
  Red = 1,
  Blue = 2,
  Green = 4,
  Yellow = 8
}

public readonly record struct TileCoordinate(int X, int Y)
{
  public override string ToString() => $"({X},{Y})";
}

public readonly record struct WorldTile(
  bool Active,
  ushort Type,
  ushort Wall,
  short FrameX,
  short FrameY,
  byte LiquidAmount,
  LiquidType LiquidType,
  byte TileColor,
  byte WallColor,
  TileWireMask Wires,
  bool Actuator,
  bool Inactive,
  bool HalfBlock = false,
  byte Slope = 0,
  byte TileHeader3 = 0,
  byte WallHeader = 0)
{
  public static WorldTile Empty => new(
    Active: false,
    Type: 0,
    Wall: 0,
    FrameX: 0,
    FrameY: 0,
    LiquidAmount: 0,
    LiquidType: LiquidType.Water,
    TileColor: 0,
    WallColor: 0,
      Wires: TileWireMask.None,
      Actuator: false,
      Inactive: false);

  public WorldTile WithLiquid(byte amount, LiquidType type)
  {
    return this with { LiquidAmount = amount, LiquidType = type };
  }
}

public readonly record struct WorldTileDelta(
  bool? Active = null,
  ushort? Type = null,
  ushort? Wall = null,
  short? FrameX = null,
  short? FrameY = null,
  byte? LiquidAmount = null,
  LiquidType? LiquidType = null,
  byte? TileColor = null,
  byte? WallColor = null,
  TileWireMask? Wires = null,
  bool? Actuator = null,
  bool? Inactive = null,
  bool? HalfBlock = null,
  byte? Slope = null,
  byte? TileHeader3 = null,
  byte? WallHeader = null)
{
  public WorldTile Apply(WorldTile current)
  {
    return current with
    {
      Active = Active ?? current.Active,
      Type = Type ?? current.Type,
      Wall = Wall ?? current.Wall,
      FrameX = FrameX ?? current.FrameX,
      FrameY = FrameY ?? current.FrameY,
      LiquidAmount = LiquidAmount ?? current.LiquidAmount,
      LiquidType = LiquidType ?? current.LiquidType,
      TileColor = TileColor ?? current.TileColor,
      WallColor = WallColor ?? current.WallColor,
      Wires = Wires ?? current.Wires,
      Actuator = Actuator ?? current.Actuator,
      Inactive = Inactive ?? current.Inactive,
      HalfBlock = HalfBlock ?? current.HalfBlock,
      Slope = Slope ?? current.Slope,
      TileHeader3 = TileHeader3 ?? current.TileHeader3,
      WallHeader = WallHeader ?? current.WallHeader
    };
  }
}

/// <summary>
/// Represents one decoded Packet 20 tile. Nullable fields preserve wire presence: an omitted
/// color, frame or liquid field must not overwrite the authoritative tile value.
/// </summary>
public readonly record struct WorldTilePatch(
  TileCoordinate Coordinate,
  bool? Active = null,
  ushort? Type = null,
  ushort? Wall = null,
  short? FrameX = null,
  short? FrameY = null,
  byte? LiquidAmount = null,
  LiquidType? LiquidType = null,
  byte? TileColor = null,
  byte? WallColor = null,
  TileWireMask? Wires = null,
  bool? Actuator = null,
  bool? Inactive = null,
  bool? HalfBlock = null,
  byte? Slope = null,
  byte? TileHeader3 = null,
  byte? WallHeader = null)
{
  public WorldTilePatch(TileCoordinate coordinate, WorldTileDelta delta)
      : this(
          coordinate,
          delta.Active,
          delta.Type,
          delta.Wall,
          delta.FrameX,
          delta.FrameY,
          delta.LiquidAmount,
          delta.LiquidType,
          delta.TileColor,
          delta.WallColor,
          delta.Wires,
          delta.Actuator,
          delta.Inactive,
          delta.HalfBlock,
          delta.Slope,
          delta.TileHeader3,
          delta.WallHeader) {
  }

  public WorldTileDelta ToDelta()
  {
    return new WorldTileDelta(
      Active,
      Type,
      Wall,
      FrameX,
      FrameY,
      LiquidAmount,
      LiquidType,
      TileColor,
      WallColor,
      Wires,
      Actuator,
      Inactive,
      HalfBlock,
      Slope,
      TileHeader3,
      WallHeader);
  }
}
