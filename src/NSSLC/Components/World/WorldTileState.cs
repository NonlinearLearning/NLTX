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
    byte Style = 0) {
  public static WorldTileState Empty => new(false, 0, 0, 0, 0);

  public bool HasLiquid => Liquid != LiquidContactFlags.None && LiquidAmount > 0;

  public bool IsValid {
    get {
      if (Active && Type == 0) {
        return false;
      }

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
