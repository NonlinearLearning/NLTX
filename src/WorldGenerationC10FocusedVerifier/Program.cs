using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestMushroomAnchorBoundary();
      TestFallenLogHandoffBoundary();
      Console.WriteLine("C10 mushroom and fallen-log focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestMushroomAnchorBoundary()
  {
    MushroomBiomeAnchorStateComponent component =
      new(generationId: 41);
    TilePosition first = new(100, 200);
    TilePosition second = new(300, 400);

    Require(
      !MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
        component,
        first,
        patchesCommitted: false),
      "a failed mushroom patch must not append an anchor");
    Require(component.Count == 0, "failed mushroom patch must preserve count");
    Require(
      MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
        component,
        first,
        patchesCommitted: true),
      "a successful mushroom patch must append an anchor");
    Require(
      MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
        component,
        second,
        patchesCommitted: true),
      "a second successful mushroom patch must append an anchor");

    MushroomBiomeAnchorStateSnapshot snapshot =
      MushroomBiomeAnchorQuery.Snapshot(component);
    Require(snapshot.GenerationId == 41, "mushroom anchor generation");
    Require(snapshot.Capacity == 50, "mushroom anchor capacity");
    Require(snapshot.Count == 2, "mushroom anchor count");
    Require(snapshot.Positions[0] == first && snapshot.Positions[1] == second,
      "mushroom anchor paired order");
    Require(
      MushroomBiomeAnchorQuery.HasAnchorWithinDistance(
        snapshot,
        new TilePosition(599, 200),
        distance: 500),
      "mushroom anchor strict distance query");
    Require(
      !MushroomBiomeAnchorQuery.HasAnchorWithinDistance(
        snapshot,
        new TilePosition(100, -300),
        distance: 500),
      "mushroom anchor strict distance boundary");

    while (component.Count < MushroomBiomeCapacityDefinition.Capacity)
    {
      Require(
        MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
          component,
          new TilePosition(component.Count, component.Count + 1),
          patchesCommitted: true),
        "mushroom anchor fill");
    }

    Require(
      !MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
        component,
        new TilePosition(999, 1000),
        patchesCommitted: true),
      "mushroom anchor capacity rejection");
    MushroomBiomeAnchorStateSnapshot beforeClear =
      MushroomBiomeAnchorQuery.Snapshot(component);
    MushroomBiomeGenerationSystem.Clear(component);
    Require(component.Count == 0, "mushroom anchor clear");
    Require(beforeClear.Count == MushroomBiomeCapacityDefinition.Capacity,
      "mushroom anchor snapshot isolation");
  }

  private static void TestFallenLogHandoffBoundary()
  {
    FallenLogFlowerHandoffComponent component =
      new(generationId: 42);

    FallenLogFlowerHandoffSnapshot initial =
      FallenLogFlowerHandoffQuery.Snapshot(component);
    Require(initial.GenerationId == 42, "fallen-log generation");
    Require(initial.LogX == -1 && initial.LogY == -1,
      "fallen-log reset sentinel");
    Require(!initial.HasPendingLog, "fallen-log initial pending state");
    Require(
      !FallenLogFlowerHandoffSystem.TryPublish(
        component,
        new TilePosition(10, 20),
        placementSucceeded: false,
        randomSelectionAccepted: true),
      "failed fallen-log placement must not publish");
    Require(
      !FallenLogFlowerHandoffSystem.TryPublish(
        component,
        new TilePosition(30, 40),
        placementSucceeded: true,
        randomSelectionAccepted: false),
      "unselected fallen-log placement must not publish");

    TilePosition first = new(50, 60);
    TilePosition replacement = new(70, 80);
    Require(
      FallenLogFlowerHandoffSystem.TryPublish(
        component,
        first,
        placementSucceeded: true,
        randomSelectionAccepted: true),
      "successful fallen-log placement must publish");
    Require(
      FallenLogFlowerHandoffSystem.TryPublish(
        component,
        replacement,
        placementSucceeded: true,
        randomSelectionAccepted: true),
      "a later selected fallen log must replace the pending handoff");

    FallenLogFlowerHandoffSnapshot pending =
      FallenLogFlowerHandoffQuery.Snapshot(component);
    Require(pending.HasPendingLog, "fallen-log pending state");
    Require(pending.LogX == replacement.X && pending.LogY == replacement.Y,
      "fallen-log replacement coordinates");

    Require(
      FallenLogFlowerHandoffSystem.TryConsume(
        component,
        out TilePosition consumed),
      "fallen-log handoff must be consumed once");
    Require(consumed == replacement, "fallen-log consumed coordinates");
    FallenLogFlowerHandoffSnapshot afterConsume =
      FallenLogFlowerHandoffQuery.Snapshot(component);
    Require(afterConsume.LogX == -1, "fallen-log consume sentinel");
    Require(afterConsume.LogY == replacement.Y,
      "fallen-log consume must preserve stale logY");
    Require(
      !FallenLogFlowerHandoffSystem.TryConsume(
        component,
        out _),
      "fallen-log handoff must not be consumed twice");

    FallenLogFlowerHandoffSystem.Reset(component);
    FallenLogFlowerHandoffSnapshot afterReset =
      FallenLogFlowerHandoffQuery.Snapshot(component);
    Require(afterReset.LogX == -1 && afterReset.LogY == -1,
      "fallen-log reset must clear both coordinates");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
