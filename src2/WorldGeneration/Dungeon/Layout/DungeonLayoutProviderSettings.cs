using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Layout;

public sealed class DungeonLayoutProviderSettings
{
  public DungeonLayoutProviderSettings(
    DungeonStyleMaterialDefinition styleData,
    int steps,
    int maxSteps)
  {
    StyleData = styleData ?? throw new ArgumentNullException(nameof(styleData));
    if (steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(steps));
    }

    if (maxSteps < steps)
    {
      throw new ArgumentOutOfRangeException(nameof(maxSteps));
    }

    Steps = steps;
    MaxSteps = maxSteps;
  }

  public DungeonStyleMaterialDefinition StyleData { get; }

  public int Steps { get; }

  public int MaxSteps { get; }
}
