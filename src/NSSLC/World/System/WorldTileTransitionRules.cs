using Terraria.Share.Liquid;

namespace Terraria.Unshared.World;

/// <summary>
/// Pure tile transition rules: what a tile becomes after a break or a place, and whether the
/// operation is applicable to the current facts.
/// </summary>
/// <remarks>
/// <see cref="WorldRuntime" /> owns the lock, the revision and the storage write; the resulting
/// tile value and the applicability decision are computed here.
/// </remarks>
public static class WorldTileTransitionRules
{
  /// <summary>True when the tile currently holds something that can be removed.</summary>
  public static bool CanBreak(WorldTileState current)
  {
    return current.Active;
  }

  /// <summary>True when the tile is free for a placement.</summary>
  public static bool CanPlace(WorldTileState current)
  {
    return !current.Active;
  }

  /// <summary>The tile value produced by breaking the supplied facts.</summary>
  public static WorldTileState Broken(WorldTileState current)
  {
    return current with { Active = false, Type = 0, FrameX = 0, FrameY = 0 };
  }

  /// <summary>The tile value produced by placing the supplied tile type and style.</summary>
  public static WorldTileState Placed(
      WorldTileState current,
      ushort tileType,
      byte style)
  {
    return current with
    {
      Active = true,
      Type = tileType,
      FrameX = 0,
      FrameY = 0,
      Style = style
    };
  }

  /// <summary>The tile value produced by writing one bounded liquid fact.</summary>
  public static WorldTileState WithLiquid(
      WorldTileState current,
      LiquidContactFlags liquid,
      byte amount)
  {
    return current with { Liquid = liquid, LiquidAmount = amount };
  }
}
