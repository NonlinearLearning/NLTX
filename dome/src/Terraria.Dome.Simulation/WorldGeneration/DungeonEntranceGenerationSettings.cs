using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonEntranceGenerationSettings
{
  public DungeonEntranceGenerationSettings(
    DungeonEntranceType entranceType,
    int randomSeed,
    DungeonStyleLookupEntry style,
    bool precalculateEntrancePosition = false)
  {
    if (!Enum.IsDefined(entranceType))
    {
      throw new ArgumentOutOfRangeException(nameof(entranceType));
    }

    ArgumentNullException.ThrowIfNull(style);
    EntranceType = entranceType;
    RandomSeed = randomSeed;
    Style = style;
    PrecalculateEntrancePosition = precalculateEntrancePosition;
  }

  public DungeonEntranceType EntranceType { get; }

  public int RandomSeed { get; }

  public DungeonStyleLookupEntry Style { get; }

  public bool PrecalculateEntrancePosition { get; }
}
