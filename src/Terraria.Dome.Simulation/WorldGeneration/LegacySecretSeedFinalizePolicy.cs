using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySecretSeedFinalizePolicy
{
  private static readonly IReadOnlyList<LegacySecretSeedFinalizeAction> Actions =
    Array.AsReadOnly(
    [
      LegacySecretSeedFinalizeAction.SurfaceIsDesertFinish,
      LegacySecretSeedFinalizeAction.ExtraLiquidFinish,
      LegacySecretSeedFinalizeAction.SurfaceIsInSpace,
      LegacySecretSeedFinalizeAction.ActuallyNoTraps,
      LegacySecretSeedFinalizeAction.SurfaceIsMushrooms,
      LegacySecretSeedFinalizeAction.WorldIsFrozen,
      LegacySecretSeedFinalizeAction.NoInfection,
      LegacySecretSeedFinalizeAction.HallowOnSurface,
      LegacySecretSeedFinalizeAction.WorldIsInfected,
      LegacySecretSeedFinalizeAction.StartInHardmode,
      LegacySecretSeedFinalizeAction.NoSurface,
      LegacySecretSeedFinalizeAction.CoatEverythingEcho,
      LegacySecretSeedFinalizeAction.CoatEverythingIlluminant,
      LegacySecretSeedFinalizeAction.PaintEverythingGray,
      LegacySecretSeedFinalizeAction.PaintEverythingNegative,
      LegacySecretSeedFinalizeAction.RandomSpawn,
      LegacySecretSeedFinalizeAction.RainbowStuff,
      LegacySecretSeedFinalizeAction.PortalGunInChests,
      LegacySecretSeedFinalizeAction.WorldIsFrozenFinish,
      LegacySecretSeedFinalizeAction.ErrorWorld,
      LegacySecretSeedFinalizeAction.TeamBasedSpawns,
      LegacySecretSeedFinalizeAction.NoSpiderCaves
    ]);

  public static IReadOnlyList<LegacySecretSeedFinalizeAction> CreateActions(
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    List<LegacySecretSeedFinalizeAction> result = new();
    foreach (LegacySecretSeedFinalizeAction action in Actions)
    {
      if (action == LegacySecretSeedFinalizeAction.SurfaceIsMushrooms &&
          enabledVariants.Contains("surface-is-mushrooms") &&
          !enabledVariants.Contains("no-surface"))
      {
        result.Add(action);
      }

      if (ShouldInclude(action, enabledVariants))
      {
        result.Add(action);
      }
    }

    return result.AsReadOnly();
  }

  private static bool ShouldInclude(
    LegacySecretSeedFinalizeAction action,
    IReadOnlySet<string> enabledVariants)
  {
    return action switch
    {
      LegacySecretSeedFinalizeAction.SurfaceIsDesertFinish =>
        enabledVariants.Contains("surface-is-desert"),
      LegacySecretSeedFinalizeAction.ExtraLiquidFinish =>
        enabledVariants.Contains("extra-liquid"),
      LegacySecretSeedFinalizeAction.SurfaceIsInSpace =>
        enabledVariants.Contains("surface-is-in-space"),
      LegacySecretSeedFinalizeAction.ActuallyNoTraps =>
        enabledVariants.Contains("actually-no-traps"),
      LegacySecretSeedFinalizeAction.SurfaceIsMushrooms =>
        enabledVariants.Contains("surface-is-mushrooms"),
      LegacySecretSeedFinalizeAction.WorldIsFrozen =>
        enabledVariants.Contains("world-is-frozen"),
      LegacySecretSeedFinalizeAction.NoInfection =>
        enabledVariants.Contains("no-infection"),
      LegacySecretSeedFinalizeAction.HallowOnSurface =>
        enabledVariants.Contains("hallow-on-the-surface"),
      LegacySecretSeedFinalizeAction.WorldIsInfected =>
        enabledVariants.Contains("world-is-infected"),
      LegacySecretSeedFinalizeAction.StartInHardmode =>
        enabledVariants.Contains("start-in-hardmode"),
      LegacySecretSeedFinalizeAction.NoSurface =>
        enabledVariants.Contains("no-surface"),
      LegacySecretSeedFinalizeAction.CoatEverythingEcho =>
        enabledVariants.Contains("coat-everything-echo"),
      LegacySecretSeedFinalizeAction.CoatEverythingIlluminant =>
        enabledVariants.Contains("coat-everything-illuminant"),
      LegacySecretSeedFinalizeAction.PaintEverythingGray =>
        enabledVariants.Contains("paint-everything-gray"),
      LegacySecretSeedFinalizeAction.PaintEverythingNegative =>
        enabledVariants.Contains("paint-everything-negative"),
      LegacySecretSeedFinalizeAction.RandomSpawn =>
        enabledVariants.Contains("random-spawn"),
      LegacySecretSeedFinalizeAction.RainbowStuff =>
        enabledVariants.Contains("rainbow-stuff"),
      LegacySecretSeedFinalizeAction.PortalGunInChests =>
        enabledVariants.Contains("portal-gun-in-chests"),
      LegacySecretSeedFinalizeAction.WorldIsFrozenFinish =>
        enabledVariants.Contains("world-is-frozen"),
      LegacySecretSeedFinalizeAction.ErrorWorld =>
        enabledVariants.Contains("error-world"),
      LegacySecretSeedFinalizeAction.TeamBasedSpawns =>
        enabledVariants.Contains("team-based-spawns"),
      LegacySecretSeedFinalizeAction.NoSpiderCaves =>
        enabledVariants.Contains("no-spider-caves"),
      _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
  }
}
