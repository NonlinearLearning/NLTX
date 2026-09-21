namespace Terraria.WorldSession.Seeds;

public static class SpecialSeedEligibilityQuery
{
  public static bool ShouldDropExtraGel(SpecialSeedRuleInput input)
  {
    return input.Flags.TenthAnniversaryWorld &&
      input.Flags.DrunkWorld &&
      !input.Flags.RemixWorld &&
      !input.Flags.NotTheBeesWorld;
  }

  public static bool ShouldDropExtraWood(SpecialSeedRuleInput input)
  {
    return ShouldDropExtraGel(input);
  }

  public static bool DungeonEntranceHasATree(SpecialSeedRuleInput input)
  {
    return input.Flags.DrunkWorld && !NoDungeonGuardian(input);
  }

  public static bool DungeonEntranceIsBuried(SpecialSeedRuleInput input)
  {
    return input.SurfaceIsDesert && !DungeonEntranceIsUnderground(input);
  }

  public static bool DungeonEntranceIsUnderground(SpecialSeedRuleInput input)
  {
    return input.Flags.DrunkWorld || input.NoSurface;
  }

  public static bool NoDungeonGuardian(SpecialSeedRuleInput input)
  {
    return input.OnlyShimmerOceanWorlds;
  }

  public static bool BossesKeepSpawning(SpecialSeedRuleInput input)
  {
    return input.Flags.GoodWorld &&
      input.Flags.DontStarveWorld &&
      !input.Flags.TenthAnniversaryWorld;
  }

  public static bool ShimmerSpawnHalfOfWorld(SpecialSeedRuleInput input)
  {
    return input.OnlyShimmerOceanWorlds;
  }

  public static bool RainbowSandAndBlackSandWalls(SpecialSeedRuleInput input)
  {
    return input.OnlyShimmerOceanWorlds;
  }

  public static bool SpawnOnBeach(SpecialSeedRuleInput input)
  {
    return input.Flags.TenthAnniversaryWorld &&
      !input.Flags.RemixWorld &&
      !input.Flags.DontStarveWorld;
  }

  public static bool SpawnOnBeachOnDungeonSide(SpecialSeedRuleInput input)
  {
    return SpawnOnBeach(input) && input.OnlyShimmerOceanWorlds;
  }

  public static bool Mechdusa(SpecialSeedRuleInput input)
  {
    return input.Flags.RemixWorld && input.Flags.GoodWorld;
  }
}
