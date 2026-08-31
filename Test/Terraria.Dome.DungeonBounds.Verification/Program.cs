using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonBoundsSnapshot bounds = DungeonBoundsSnapshot.Create(
  left: 20,
  top: 30,
  right: 80,
  bottom: 90,
  worldWidth: 400,
  worldHeight: 300);
if (bounds.Width != 60 || bounds.Height != 60 || bounds.Size != 60 ||
    bounds.CenterX != 50 || bounds.CenterY != 60 ||
    !bounds.Contains(20, 30) || bounds.Contains(80, 90) ||
    !bounds.ContainsWithFluff(15, 25, 5) || bounds.ContainsWithFluff(14, 25, 5) ||
    !bounds.Intersects(DungeonBoundsSnapshot.Create(70, 80, 100, 110, 400, 300)) ||
    bounds.Intersects(DungeonBoundsSnapshot.Create(80, 90, 100, 110, 400, 300)) ||
    !bounds.IntersectsLineThreePointCheck(0, 0, 100, 100))
{
  throw new InvalidOperationException("Dungeon bounds geometry diverged.");
}

DungeonBoundsSnapshot clamped = DungeonBoundsSnapshot.Create(-100, -100, 1000, 1000, 400, 300);
if (clamped.Left != 10 || clamped.Top != 10 || clamped.Right != 390 || clamped.Bottom != 290)
{
  throw new InvalidOperationException("Dungeon bounds world clamp diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => bounds.ContainsWithFluff(20, 20, -1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  DungeonBoundsSnapshot.Create(0, 0, 1, 1, 20, 300));

DungeonGenerationState dungeonGenerationState = DungeonGenerationState.Empty;
dungeonGenerationState = dungeonGenerationState.SetUp(2);
if (dungeonGenerationState.CurrentDungeon != 2 ||
    dungeonGenerationState.Contexts.Count != 1 ||
    dungeonGenerationState.Contexts[0].Sequence != 0)
{
  throw new InvalidOperationException("Dungeon generation setup did not create its first context.");
}

dungeonGenerationState = dungeonGenerationState.SetUp(4);
if (dungeonGenerationState.CurrentDungeon != 4 ||
    dungeonGenerationState.Contexts.Count != 2 ||
    dungeonGenerationState.Contexts[1].Sequence != 1)
{
  throw new InvalidOperationException("Dungeon generation setup did not append a fresh context.");
}

dungeonGenerationState = dungeonGenerationState.SetUp(6, clearOld: true);
if (dungeonGenerationState.CurrentDungeon != 6 ||
    dungeonGenerationState.Contexts.Count != 1 ||
    dungeonGenerationState.Contexts[0].Sequence != 0)
{
  throw new InvalidOperationException("Dungeon generation setup did not clear obsolete contexts.");
}

string[] dualDungeonDisabledPasses =
[
  "IceBiome", "DesertBiome", "Jungle", "JungleShrines", "ChestsInJungleShrines",
  "Beehives", "BeeLarvaInBeehives", "LihzahrdTemple", "LihzahrdTemplePart2",
  "LihzahrdAltar", "CorruptionAndCrimson", "Shimmer"
];
if (LegacyDualDungeonPassPolicy.DisabledPassIds.Count != dualDungeonDisabledPasses.Length)
{
  throw new InvalidOperationException("Dual-dungeon disabled pass inventory diverged from source.");
}

foreach (string passId in dualDungeonDisabledPasses)
{
  if (!LegacyDualDungeonPassPolicy.ShouldDisable(passId, dualDungeonsEnabled: true) ||
      LegacyDualDungeonPassPolicy.ShouldDisable(passId, dualDungeonsEnabled: false))
  {
    throw new InvalidOperationException("Dual-dungeon pass disable policy diverged from source.");
  }
}

if (LegacyDualDungeonPassPolicy.ShouldDisable("jungle", dualDungeonsEnabled: true) ||
    LegacyDualDungeonPassPolicy.ShouldDisable("Dungeon", dualDungeonsEnabled: true))
{
  throw new InvalidOperationException("Dual-dungeon pass matching did not preserve ordinal pass names.");
}

IReadOnlySet<string> enabledSecretSeeds = new HashSet<string>(StringComparer.Ordinal)
{
  "vampirism",
  "world-is-infected",
  "team-based-spawns",
  "dual-dungeons",
  "endless-halloween",
  "endless-christmas"
};
SecretSeedRuntimeProjection secretSeedProjection = SecretSeedRuntimeProjection.Create(
  enabledSecretSeeds);
if (!secretSeedProjection.VampireSeed || !secretSeedProjection.InfectedSeed ||
    !secretSeedProjection.TeamBasedSpawnsSeed || !secretSeedProjection.DualDungeonsSeed ||
    !secretSeedProjection.ForceHalloweenForever || !secretSeedProjection.ForceXmasForever)
{
  throw new InvalidOperationException("Secret-seed runtime projection diverged from source.");
}

SecretSeedRuntimeProjection defaultSecretSeedProjection = SecretSeedRuntimeProjection.Create(
  new HashSet<string>(StringComparer.Ordinal));
if (defaultSecretSeedProjection != default)
{
  throw new InvalidOperationException("Default secret-seed runtime projection was not empty.");
}

IReadOnlyList<LegacySecretSeedFinalizeAction> mushroomFinalization =
  LegacySecretSeedFinalizePolicy.CreateActions(
    new HashSet<string>(StringComparer.Ordinal) { "surface-is-mushrooms" });
if (mushroomFinalization.Count != 2 ||
    mushroomFinalization[0] != LegacySecretSeedFinalizeAction.SurfaceIsMushrooms ||
    mushroomFinalization[1] != LegacySecretSeedFinalizeAction.SurfaceIsMushrooms)
{
  throw new InvalidOperationException("Mushroom secret-seed finalization call count diverged.");
}

IReadOnlyList<LegacySecretSeedFinalizeAction> noSurfaceMushroomFinalization =
  LegacySecretSeedFinalizePolicy.CreateActions(
    new HashSet<string>(StringComparer.Ordinal) { "surface-is-mushrooms", "no-surface" });
if (noSurfaceMushroomFinalization.Count != 2 ||
    noSurfaceMushroomFinalization[0] != LegacySecretSeedFinalizeAction.SurfaceIsMushrooms ||
    noSurfaceMushroomFinalization[1] != LegacySecretSeedFinalizeAction.NoSurface)
{
  throw new InvalidOperationException("No-surface mushroom finalization order diverged.");
}

IReadOnlyList<LegacySecretSeedFinalizeAction> frozenFinalization =
  LegacySecretSeedFinalizePolicy.CreateActions(
    new HashSet<string>(StringComparer.Ordinal)
    {
      "surface-is-in-space",
      "no-infection",
      "world-is-frozen"
    });
if (frozenFinalization.Count != 4 ||
    frozenFinalization[0] != LegacySecretSeedFinalizeAction.SurfaceIsInSpace ||
    frozenFinalization[1] != LegacySecretSeedFinalizeAction.WorldIsFrozen ||
    frozenFinalization[2] != LegacySecretSeedFinalizeAction.NoInfection ||
    frozenFinalization[3] != LegacySecretSeedFinalizeAction.WorldIsFrozenFinish)
{
  throw new InvalidOperationException("Secret-seed finalization order diverged from source.");
}

Console.WriteLine("PASS: DungeonBounds immutable geometry contract");
Console.WriteLine("PASS: dungeon generation setup preserves immutable context reset semantics");
Console.WriteLine("PASS: dual-dungeon pass disable policy preserves legacy pass names");
Console.WriteLine("PASS: secret-seed runtime projection preserves legacy flags");
Console.WriteLine("PASS: secret-seed finalization policy preserves source order");

static void AssertThrows<TException>(Action action)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
