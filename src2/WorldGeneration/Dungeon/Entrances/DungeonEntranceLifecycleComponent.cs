using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Entrances;

public sealed class DungeonEntranceLifecycleComponent
{
  public bool Calculated { get; private set; }

  public bool Generated { get; private set; }

  public DungeonBoundsRectangle Bounds { get; private set; }

  public DungeonTilePoint OldManSpawn { get; private set; }

  public void MarkCalculated(
    DungeonBoundsRectangle bounds,
    DungeonTilePoint oldManSpawn)
  {
    if (bounds.IsEmpty)
    {
      throw new ArgumentException("Entrance bounds must be non-empty.", nameof(bounds));
    }

    Bounds = bounds;
    OldManSpawn = oldManSpawn;
    Calculated = true;
  }

  public void MarkGenerated()
  {
    if (!Calculated)
    {
      throw new InvalidOperationException(
        "An entrance cannot be generated before it is calculated.");
    }

    Generated = true;
  }

  public void Reset()
  {
    Calculated = false;
    Generated = false;
    Bounds = default;
    OldManSpawn = default;
  }
}
