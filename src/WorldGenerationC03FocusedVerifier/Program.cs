using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestSpawnAndLandmassSnapshotIsolation();
      TestLandmassDefinitionAdapter();
      TestSurfaceMaterialDefaults();
      TestSurfaceMaterialFlip();
      TestLiquidBoundaryCommit();
      Console.WriteLine("C03 spawn, material, and liquid-boundary focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestSpawnAndLandmassSnapshotIsolation()
  {
    List<WorldGenerationLandmassValue> source =
    [
      new WorldGenerationLandmassValue(1, 10.5, 20.5, 30, 2)
    ];
    WorldSpawnAndLandmassComponent component =
      new(
        generationId: 81,
        worldSpawnHasBeenRandomized: true,
        landmassData: source,
        remixSurfaceLayerLow: 100,
        remixSurfaceLayerHigh: 200,
        remixMushroomLayerLow: 300,
        remixMushroomLayerHigh: 400,
        boulderPetsPlaced: 2);

    source[0] = new WorldGenerationLandmassValue(9, 90, 90, 9, 9);
    WorldSpawnAndLandmassSnapshot snapshot = component.CreateSnapshot();
    Require(snapshot.GenerationId == 81, "spawn snapshot generation identity");
    Require(snapshot.WorldSpawnHasBeenRandomized, "spawn randomized flag");
    Require(snapshot.LandmassData.Count == 1, "landmass count");
    Require(
      snapshot.LandmassData[0] == new WorldGenerationLandmassValue(1, 10.5, 20.5, 30, 2),
      "landmass input must be copied");
    Require(
      ((IList<WorldGenerationLandmassValue>)snapshot.LandmassData).IsReadOnly,
      "landmass snapshot must be read-only");
    Require(snapshot.LandmassData[0].TopY == -9.5, "landmass top calculation");
    Require(
      WorldSpawnAndLandmassQuery.Snapshot(component).LandmassData.Count == 1,
      "spawn query returns a copied snapshot");

    WorldSpawnAndLandmassSystem.Commit(
      component,
      snapshot with
      {
        BoulderPetsPlaced = 3,
        RemixSurfaceLayerHigh = 250
      });
    Require(component.BoulderPetsPlaced == 3, "spawn commit replaces counter");
    Require(component.RemixSurfaceLayerHigh == 250, "spawn commit replaces remix layer");
    RequireThrows<ArgumentException>(
      () => WorldSpawnAndLandmassSystem.Commit(
        component,
        snapshot with { GenerationId = 82 }),
      "spawn commit must reject a stale generation");
  }

  private static void TestLandmassDefinitionAdapter()
  {
    List<WorldLandmassDefinition> definitions =
    [
      new WorldLandmassDefinition(
        WorldLandmassDataType.RoundLandmass,
        new Vector2(10.5f, 20.5f),
        30,
        2),
      new WorldLandmassDefinition(
        WorldLandmassDataType.ExtraLiquidBubbleSquare,
        new Vector2(40.5f, 50.5f),
        8,
        0)
    ];

    IReadOnlyList<WorldGenerationLandmassValue> values =
      WorldLandmassDefinitionAdapter.ToValues(definitions);
    definitions[0] = new WorldLandmassDefinition(
      WorldLandmassDataType.SkyblockIsland,
      new Vector2(90f, 90f),
      9,
      9);

    Require(values.Count == 2, "landmass adapter count");
    Require(
      values[0] == new WorldGenerationLandmassValue(0, 10.5, 20.5, 30, 2),
      "landmass adapter maps the source fields");
    Require(
      values[1] == new WorldGenerationLandmassValue(2, 40.5, 50.5, 8, 0),
      "landmass adapter maps the data type");
    Require(
      ((IList<WorldGenerationLandmassValue>)values).IsReadOnly,
      "landmass adapter returns a read-only copy");
  }

  private static void TestSurfaceMaterialDefaults()
  {
    SurfaceMaterialDefinition materials = SurfaceMaterialDefinition.Version4;
    Require(materials.CrimsonStoneWall == 83, "crimson wall default");
    Require(materials.CrimsonStone == 203, "crimson stone default");
    Require(materials.EbonStoneWall == 3, "ebon wall default");
    Require(materials.EbonStone == 25, "ebon stone default");
    Require(materials.MossTile == 179, "moss tile default");
    Require(materials.MossWall == 54, "moss wall default");
  }

  private static void TestSurfaceMaterialFlip()
  {
    SurfaceMaterialDefinition defaults = SurfaceMaterialDefinition.Version4;
    SurfaceMaterialDefinition unchanged =
      SurfaceMaterialDefinitionQuery.ApplyInfectionFlip(defaults, false);
    SurfaceMaterialDefinition flipped =
      SurfaceMaterialDefinitionQuery.ApplyInfectionFlip(defaults, true);

    Require(unchanged == defaults, "material query preserves normal infection alignment");
    Require(flipped.CrimsonStoneWall == defaults.EbonStoneWall, "flipped crimson wall");
    Require(flipped.CrimsonStone == defaults.EbonStone, "flipped crimson stone");
    Require(flipped.EbonStoneWall == defaults.CrimsonStoneWall, "flipped ebon wall");
    Require(flipped.EbonStone == defaults.CrimsonStone, "flipped ebon stone");
    Require(flipped.MossTile == defaults.MossTile, "infection flip preserves moss tile");
    Require(flipped.MossWall == defaults.MossWall, "infection flip preserves moss wall");
  }

  private static void TestLiquidBoundaryCommit()
  {
    WorldGenerationLiquidBoundaryComponent component =
      new(generationId: 83, lavaLine: 100, waterLine: 200);
    WorldGenerationLiquidBoundarySnapshot snapshot = component.CreateSnapshot();
    Require(snapshot.LavaLine == 100 && snapshot.WaterLine == 200, "liquid snapshot values");
    Require(
      WorldGenerationLiquidBoundaryQuery.Snapshot(component).WaterLine == 200,
      "liquid query returns the boundary snapshot");

    WorldGenerationLiquidBoundarySystem.Commit(
      component,
      new WorldGenerationLiquidBoundarySnapshot(83, 300, 400));
    Require(component.LavaLine == 300, "lava boundary commit");
    Require(component.WaterLine == 400, "water boundary commit");
    RequireThrows<ArgumentException>(
      () => WorldGenerationLiquidBoundarySystem.Commit(
        component,
        new WorldGenerationLiquidBoundarySnapshot(84, 1, 2)),
      "liquid commit must reject a stale generation");
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
