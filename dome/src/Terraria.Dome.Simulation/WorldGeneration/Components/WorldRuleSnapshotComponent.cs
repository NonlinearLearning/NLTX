using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldRuleSnapshotComponent
{
  public WorldRuleSnapshotComponent(
    int difficulty,
    string secretSeedVariant,
    bool isHardmode,
    bool isRemixWorld = false,
    bool isEverythingWorld = false,
    bool isNoTrapsWorld = false,
    bool isDrunkWorld = false,
    bool isGoodWorld = false,
    bool isDontStarveWorld = false,
    bool isNotTheBeesWorld = false,
    bool isSkyblockWorld = false,
    bool isNoSurfaceWorld = false,
    bool isSurfaceDesertWorld = false,
    bool isIceBiomeWorld = false,
    bool isTenthAnniversaryWorld = false,
    bool noInfection = false,
    bool extraLiquid = false,
    bool worldIsFrozen = false,
    bool startInHardmode = false)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(secretSeedVariant);
    if (difficulty < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    Difficulty = difficulty;
    SecretSeedVariant = secretSeedVariant;
    IsHardmode = isHardmode;
    IsRemixWorld = isRemixWorld;
    IsEverythingWorld = isEverythingWorld;
    IsNoTrapsWorld = isNoTrapsWorld;
    IsDrunkWorld = isDrunkWorld;
    IsGoodWorld = isGoodWorld;
    IsDontStarveWorld = isDontStarveWorld;
    IsNotTheBeesWorld = isNotTheBeesWorld;
    IsSkyblockWorld = isSkyblockWorld;
    IsNoSurfaceWorld = isNoSurfaceWorld;
    IsSurfaceDesertWorld = isSurfaceDesertWorld;
    IsIceBiomeWorld = isIceBiomeWorld;
    IsTenthAnniversaryWorld = isTenthAnniversaryWorld;
    NoInfection = noInfection;
    ExtraLiquid = extraLiquid;
    WorldIsFrozen = worldIsFrozen;
    StartInHardmode = startInHardmode;
  }

  public int Difficulty { get; }
  public string SecretSeedVariant { get; }
  public bool IsHardmode { get; }

  public bool IsRemixWorld { get; }

  public bool IsEverythingWorld { get; }

  public bool IsNoTrapsWorld { get; }

  public bool IsDrunkWorld { get; }

  public bool IsGoodWorld { get; }

  public bool IsDontStarveWorld { get; }

  public bool IsNotTheBeesWorld { get; }

  public bool IsSkyblockWorld { get; }

  public bool IsNoSurfaceWorld { get; }

  public bool IsSurfaceDesertWorld { get; }

  public bool IsIceBiomeWorld { get; }

  public bool IsTenthAnniversaryWorld { get; }

  public bool NoInfection { get; }

  public bool ExtraLiquid { get; }

  public bool WorldIsFrozen { get; }

  public bool StartInHardmode { get; }
}
