using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldGenerationRulesComponent
{
  public WorldGenerationRulesComponent(
    int difficulty,
    string seedVariant,
    WorldEvilType? worldEvil = null)
  {
    if (difficulty < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    Difficulty = difficulty;
    SeedVariant = seedVariant;
    WorldEvil = worldEvil;
  }

  public int Difficulty { get; }

  public string SeedVariant { get; }

  public WorldEvilType? WorldEvil { get; }

  public bool IsRemixWorld { get; init; }

  public bool IsEverythingWorld { get; init; }

  public bool IsNoTrapsWorld { get; init; }

  public bool IsDrunkWorld { get; init; }

  public bool IsGoodWorld { get; init; }

  public bool IsDontStarveWorld { get; init; }

  public bool IsNotTheBeesWorld { get; init; }

  public bool IsSkyblockWorld { get; init; }

  public bool IsNoSurfaceWorld { get; init; }

  public bool IsSurfaceDesertWorld { get; init; }

  public bool IsIceBiomeWorld { get; init; }

  public bool IsTenthAnniversaryWorld { get; init; }

  public bool NoInfection { get; init; }

  public bool ExtraLiquid { get; init; }

  public bool WorldIsFrozen { get; init; }

  public bool StartInHardmode { get; init; }
}
