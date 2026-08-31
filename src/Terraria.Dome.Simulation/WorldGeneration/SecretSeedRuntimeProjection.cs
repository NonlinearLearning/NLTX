using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SecretSeedRuntimeProjection(
  bool VampireSeed,
  bool InfectedSeed,
  bool TeamBasedSpawnsSeed,
  bool DualDungeonsSeed,
  bool ForceHalloweenForever,
  bool ForceXmasForever)
{
  private const string DualDungeonsVariant = "dual-dungeons";
  private const string EndlessChristmasVariant = "endless-christmas";
  private const string EndlessHalloweenVariant = "endless-halloween";
  private const string TeamBasedSpawnsVariant = "team-based-spawns";
  private const string VampirismVariant = "vampirism";
  private const string WorldIsInfectedVariant = "world-is-infected";

  public static SecretSeedRuntimeProjection Create(IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return new SecretSeedRuntimeProjection(
      enabledVariants.Contains(VampirismVariant),
      enabledVariants.Contains(WorldIsInfectedVariant),
      enabledVariants.Contains(TeamBasedSpawnsVariant),
      enabledVariants.Contains(DualDungeonsVariant),
      enabledVariants.Contains(EndlessHalloweenVariant),
      enabledVariants.Contains(EndlessChristmasVariant));
  }
}
