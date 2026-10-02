namespace Terraria.WorldGeneration.Dungeon.Bounds;

public sealed class DungeonBoundsMutationSystem
{
  public DungeonBoundsMutationResult SetBounds(
    DungeonBoundsComponent bounds,
    int left,
    int right,
    int top,
    int bottom)
  {
    ArgumentNullException.ThrowIfNull(bounds);

    int clampedLeft = Math.Clamp(left, 0, bounds.WorldWidth);
    int clampedRight = Math.Clamp(right, 0, bounds.WorldWidth);
    int clampedTop = Math.Clamp(top, 0, bounds.WorldHeight);
    int clampedBottom = Math.Clamp(bottom, 0, bounds.WorldHeight);

    if (clampedRight <= clampedLeft || clampedBottom <= clampedTop)
    {
      return DungeonBoundsMutationResult.Rejected(
        "Dungeon bounds must have positive width and height.");
    }

    bounds.Replace(clampedLeft, clampedRight, clampedTop, clampedBottom);
    return DungeonBoundsMutationResult.Accepted();
  }

  public DungeonBoundsMutationResult Reset(DungeonBoundsComponent bounds)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    bounds.Replace(0, 0, 0, 0);
    return DungeonBoundsMutationResult.Accepted();
  }
}
