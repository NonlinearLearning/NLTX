using Terraria.Liquid;

namespace Terraria.World;

/// <summary>Read-only tile value returned by the world interaction boundary.</summary>
public readonly record struct WorldTileState(
    bool Active,
    ushort Type,
    ushort Wall,
    short FrameX,
    short FrameY,
    LiquidContactFlags Liquid = LiquidContactFlags.None,
    byte LiquidAmount = 0,
    byte Style = 0,
    byte WallColor = 0,
    byte Wires = 0,
    bool Actuator = false,
    bool Inactive = false,
    bool HalfBlock = false,
    byte Slope = 0,
    byte TileHeader3 = 0,
    byte WallHeader = 0) {
  public static WorldTileState Empty => new(false, 0, 0, 0, 0);

  /// <summary>Packet 10 tile paint, retained under the historical component name.</summary>
  public byte TileColor => Style;

  public bool HasLiquid => Liquid != LiquidContactFlags.None && LiquidAmount > 0;

  public bool IsValid {
    get {
      const LiquidContactFlags knownLiquids =
          LiquidContactFlags.Water |
          LiquidContactFlags.Honey |
          LiquidContactFlags.Lava |
          LiquidContactFlags.Shimmer;
      byte liquidValue = (byte)Liquid;
      bool isSingleLiquid = liquidValue == 0 || (liquidValue & (liquidValue - 1)) == 0;
      return (Liquid & ~knownLiquids) == LiquidContactFlags.None &&
          isSingleLiquid &&
          ((Liquid == LiquidContactFlags.None) == (LiquidAmount == 0));
    }
  }

  public void Deconstruct(
      out bool active,
      out ushort type,
      out ushort wall,
      out short frameX,
      out short frameY)
  {
    active = Active;
    type = Type;
    wall = Wall;
    frameX = FrameX;
    frameY = FrameY;
  }
}
