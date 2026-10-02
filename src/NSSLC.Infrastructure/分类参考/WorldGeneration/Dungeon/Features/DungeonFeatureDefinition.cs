using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Features;

public sealed class DungeonFeatureDefinition
{
  public DungeonFeatureDefinition(int featureId, bool isGlobal = false)
  {
    if (featureId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(featureId));
    }

    FeatureId = featureId;
    IsGlobal = isGlobal;
  }

  public int FeatureId { get; }

  public bool IsGlobal { get; }

  public DungeonBoundsRectangle? Bounds { get; private set; }

  public void SetBounds(DungeonBoundsRectangle bounds)
  {
    if (IsGlobal)
    {
      throw new InvalidOperationException(
        "Global dungeon features do not own room bounds.");
    }

    if (bounds.IsEmpty)
    {
      throw new ArgumentException("Feature bounds must be non-empty.", nameof(bounds));
    }

    Bounds = bounds;
  }
}
