using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestConstraintCalculationFromBeachBoundary();
      TestConstraintSnapshotAndCommit();
      TestBoundedTreasureRecording();
      TestVersion4TreasureAttemptOverflowReset();
      TestTreasurePlacementProjection();
      TestTreasurePlacementInputBoundary();
      TestPassControlReset();
      Console.WriteLine("C05 ocean-biome focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestConstraintCalculationFromBeachBoundary()
  {
    BeachBoundarySnapshot beachBoundary = new(
      GenerationId: 95,
      LeftBeachEnd: 300,
      RightBeachStart: 700,
      BeachBordersWidth: 275,
      BeachSandRandomCenter: 320,
      BeachSandRandomWidthRange: 20,
      BeachSandDungeonExtraWidth: 40,
      BeachSandJungleExtraWidth: 20,
      ShellStartXLeft: 40,
      ShellStartYLeft: 100,
      ShellStartXRight: 960,
      ShellStartYRight: 100,
      OceanWaterStartRandomMin: 220);
    OceanBiomeConstraintCalculationInput input =
      new(beachBoundary);

    OceanBiomeConstraintSnapshot calculated =
      OceanBiomeConstraintCalculationQuery.Calculate(in input);
    Require(calculated.GenerationId == 95, "calculated constraint generation identity");
    Require(calculated.OceanWaterStartRandomMax == 260, "calculated ocean random maximum");
    Require(
      calculated.OceanWaterForcedJungleLength == 275,
      "calculated forced jungle length");
    Require(calculated.EvilBiomeBeachAvoidance == 380, "calculated evil beach avoidance");
    Require(
      calculated.EvilBiomeAvoidanceMidFixer == 50,
      "calculated evil middle fixer");
    Require(calculated.LakesBeachAvoidance == 340, "calculated lake beach avoidance");
    Require(calculated.SmallHolesBeachAvoidance == 340, "calculated small-hole avoidance");
    Require(calculated.SurfaceCavesBeachAvoidance == 340, "calculated surface-cave avoidance");
    Require(
      calculated.SurfaceCavesBeachAvoidance2 == 340,
      "calculated second surface-cave avoidance");

    OceanBiomeConstraintComponent component =
      new(generationId: 95);
    OceanBiomeConstraintSnapshot committed =
      OceanBiomeConstraintCalculationSystem.CalculateAndCommit(component, in input);
    Require(committed == calculated, "calculated constraints are committed atomically");
  }

  private static void TestConstraintSnapshotAndCommit()
  {
    OceanBiomeConstraintComponent component = new(
      generationId: 91,
      oceanWaterStartRandomMax: 1,
      oceanWaterForcedJungleLength: 2,
      evilBiomeBeachAvoidance: 3,
      evilBiomeAvoidanceMidFixer: 4,
      lakesBeachAvoidance: 5,
      smallHolesBeachAvoidance: 6,
      surfaceCavesBeachAvoidance: 7,
      surfaceCavesBeachAvoidance2: 8);

    OceanBiomeConstraintSnapshot initial =
      OceanBiomeConstraintQuery.Snapshot(component);
    Require(initial.GenerationId == 91, "constraint generation identity");
    Require(initial.OceanWaterStartRandomMax == 1, "ocean random maximum");
    Require(initial.OceanWaterForcedJungleLength == 2, "forced jungle length");
    Require(initial.EvilBiomeBeachAvoidance == 3, "evil beach avoidance");
    Require(initial.EvilBiomeAvoidanceMidFixer == 4, "evil middle fixer");
    Require(initial.LakesBeachAvoidance == 5, "lake beach avoidance");
    Require(initial.SmallHolesBeachAvoidance == 6, "small-hole avoidance");
    Require(initial.SurfaceCavesBeachAvoidance == 7, "surface-cave avoidance");
    Require(initial.SurfaceCavesBeachAvoidance2 == 8, "second surface-cave avoidance");

    OceanBiomeConstraintSnapshot replacement = new(
      GenerationId: 91,
      OceanWaterStartRandomMax: 11,
      OceanWaterForcedJungleLength: 12,
      EvilBiomeBeachAvoidance: 13,
      EvilBiomeAvoidanceMidFixer: 14,
      LakesBeachAvoidance: 15,
      SmallHolesBeachAvoidance: 16,
      SurfaceCavesBeachAvoidance: 17,
      SurfaceCavesBeachAvoidance2: 18);
    OceanBiomeConstraintSystem.Commit(component, replacement);
    Require(
      OceanBiomeConstraintQuery.Snapshot(component) == replacement,
      "constraint commit and query preserve all values");
    RequireThrows<ArgumentException>(
      () => OceanBiomeConstraintSystem.Commit(
        component,
        replacement with { GenerationId = 92 }),
      "constraint commit must reject a stale generation");
  }

  private static void TestBoundedTreasureRecording()
  {
    OceanCaveTreasureStateComponent component =
      new(generationId: 93);
    TilePosition first = new(10, 20);
    TilePosition second = new(30, 40);
    TilePosition third = new(50, 60);

    Require(OceanCaveTreasureSystem.TryAppend(component, first), "first treasure append");
    Require(OceanCaveTreasureSystem.TryAppend(component, second), "second treasure append");
    Require(
      !OceanCaveTreasureSystem.TryAppend(component, third),
      "treasure capacity rejects the third append");

    OceanCaveTreasureSnapshot snapshot = OceanCaveTreasureQuery.Snapshot(component);
    Require(snapshot.GenerationId == 93, "treasure generation identity");
    Require(snapshot.Count == 2, "treasure count at capacity");
    Require(snapshot.Positions[0] == first, "first treasure position");
    Require(snapshot.Positions[1] == second, "second treasure position");
    Require(
      ((IList<TilePosition>)snapshot.Positions).IsReadOnly,
      "treasure snapshot must be read-only");

    OceanCaveTreasureSystem.Clear(component);
    Require(
      OceanCaveTreasureQuery.Snapshot(component).Count == 0,
      "treasure clear resets the used count");
    Require(
      snapshot.Count == 2 && snapshot.Positions[0] == first,
      "treasure snapshot is isolated from later clear");
  }

  private static void TestPassControlReset()
  {
    OceanBiomePassControlComponent component =
      new(generationId: 94);
    Require(
      !OceanBiomePassControlQuery.IsDesertTileCheckSkipped(component),
      "pass control default");

    OceanBiomePassControlSystem.SetSkipDesertTileCheck(component, true);
    Require(
      OceanBiomePassControlQuery.IsDesertTileCheckSkipped(component),
      "pass control set");

    OceanBiomePassControlSystem.Reset(component);
    Require(
      !OceanBiomePassControlQuery.IsDesertTileCheckSkipped(component),
      "pass control reset");
  }

  private static void TestTreasurePlacementProjection()
  {
    OceanCaveTreasureStateComponent component =
      new(generationId: 98);
    TilePosition first = new(100, 200);
    TilePosition second = new(300, 400);
    Require(OceanCaveTreasureSystem.TryAppend(component, first), "projection first append");
    Require(OceanCaveTreasureSystem.TryAppend(component, second), "projection second append");

    OceanCaveTreasurePlacementCommand[] commands =
      OceanCaveTreasureProjection.CreateCommands(
        OceanCaveTreasureQuery.Snapshot(component),
        mainItemInChest: 863);

    Require(commands.Length == 2, "projection preserves the used treasure count");
    Require(commands[0].GenerationId == 98, "projection preserves generation identity");
    Require(commands[0].Anchor == first, "projection preserves first coordinate order");
    Require(commands[1].Anchor == second, "projection preserves second coordinate order");
    Require(commands[0].MainItemInChest == 863, "projection preserves selected item");
    Require(!commands[0].NotNearOtherChests, "projection preserves chest proximity flag");
    Require(commands[0].ChestStyle == 17, "projection preserves underwater chest style");
    Require(commands[0].TrySlope, "projection preserves slope placement flag");
    Require(commands[0].ChestTileType == 0, "projection preserves default chest tile type");

    Require(
      OceanCaveTreasureQuery.Snapshot(component).Positions[0] == first,
      "projection does not expose mutable treasure storage");
  }

  private static void TestVersion4TreasureAttemptOverflowReset()
  {
    OceanCaveTreasureStateComponent component =
      new(generationId: 97);
    TilePosition first = new(10, 20);
    TilePosition second = new(30, 40);
    TilePosition replacement = new(50, 60);

    Require(OceanCaveTreasureSystem.TryAppend(component, first), "first overflow fixture append");
    Require(OceanCaveTreasureSystem.TryAppend(component, second), "second overflow fixture append");
    Require(
      !OceanCaveTreasureSystem.RecordAfterOceanCaveAttempt(
        component,
        replacement,
        treasureGenerated: false),
      "full treasure attempt without result is rejected");
    Require(
      OceanCaveTreasureQuery.Snapshot(component).Count == 0,
      "full treasure attempt resets before an unsuccessful result");

    Require(OceanCaveTreasureSystem.TryAppend(component, first), "refill first overflow fixture");
    Require(OceanCaveTreasureSystem.TryAppend(component, second), "refill second overflow fixture");
    Require(
      OceanCaveTreasureSystem.RecordAfterOceanCaveAttempt(
        component,
        replacement,
        treasureGenerated: true),
      "full treasure attempt records the replacement result");

    OceanCaveTreasureSnapshot snapshot = OceanCaveTreasureQuery.Snapshot(component);
    Require(snapshot.Count == 1, "overflow reset keeps only the new result");
    Require(snapshot.Positions[0] == replacement, "overflow reset preserves new coordinate");
  }

  private static void TestTreasurePlacementInputBoundary()
  {
    OceanCaveTreasureSnapshot emptySnapshot = new(
      GenerationId: 99,
      Count: 0,
      Positions: Array.Empty<TilePosition>());
    OceanCaveTreasurePlacementCommand[] emptyCommands =
      OceanCaveTreasureProjection.CreateCommands(emptySnapshot, mainItemInChest: 0);
    Require(emptyCommands.Length == 0, "empty treasure projection has no commands");

    RequireThrows<ArgumentOutOfRangeException>(
      () => OceanCaveTreasureProjection.CreateCommands(emptySnapshot, mainItemInChest: -1),
      "projection rejects an invalid item even when no anchor is present");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action.Invoke();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
