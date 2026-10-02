using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Queries;

public static class DungeonBoundsQuery
{
  public static DungeonBoundsSnapshot Snapshot(DungeonBoundsComponent bounds)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    return new DungeonBoundsSnapshot(
      bounds.Left,
      bounds.Right,
      bounds.Top,
      bounds.Bottom,
      bounds.Hitbox);
  }

  public static bool Contains(
    DungeonBoundsComponent bounds,
    DungeonTilePoint point)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    return bounds.Hitbox.Contains(point);
  }

  public static bool Intersects(
    DungeonBoundsComponent bounds,
    DungeonBoundsRectangle other)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    return bounds.Hitbox.Intersects(other);
  }
}
