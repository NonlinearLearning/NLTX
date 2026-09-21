using System;
using System.Collections.Generic;
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
      TestDungeonLayoutBoundary();
      TestDungeonRewardBoundary();
      TestFloatingIslandBoundary();
      Console.WriteLine("C08 dungeon-island focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestDungeonLayoutBoundary()
  {
    DungeonLayoutControlComponent component =
      new(generationId: 801);
    List<DungeonRecordSnapshot> records = new()
    {
      new DungeonRecordSnapshot(11),
      new DungeonRecordSnapshot(22)
    };

    DungeonLayoutControlSystem.Commit(
      component,
      new DungeonLayoutSnapshot(
        GenerationId: 801,
        TLeft: 1,
        TRight: 2,
        TTop: 3,
        TBottom: 4,
        TRooms: 5,
        LAltarX: 6,
        LAltarY: 7,
        Records: records,
        CurrentDungeon: 0));

    records[0] = new DungeonRecordSnapshot(99);
    DungeonLayoutSnapshot snapshot = DungeonLayoutQuery.Snapshot(component);
    Require(snapshot.Records[0].RecordId == 11, "dungeon records must be copied on commit");
    Require(snapshot.TLeft == 1 && snapshot.LAltarY == 7, "dungeon layout scalars must be retained");
    Require(DungeonBoundaryDefinition.BeachPadding == 50, "dungeon beach padding must be 50");
    Require(((IList<DungeonRecordSnapshot>)snapshot.Records).IsReadOnly, "dungeon records must be read-only");

    DungeonSelectionControlSystem.Select(component, -1);
    Require(component.CurrentDungeon == 0, "negative dungeon selection must clamp to zero");
    DungeonSelectionControlSystem.Select(component, 1);
    Require(DungeonLayoutQuery.CurrentRecord(component).RecordId == 22, "active dungeon record must switch");

    DungeonSelectionControlSystem.Select(component, 3);
    RequireThrows<ArgumentOutOfRangeException>(
      () => DungeonLayoutQuery.CurrentRecord(component),
      "invalid active dungeon index must preserve direct index failure");

    DungeonLayoutResetSystem.Reset(component);
    DungeonLayoutSnapshot reset = DungeonLayoutQuery.Snapshot(component);
    Require(
      reset.Records.Count == 0 && reset.CurrentDungeon == 0 && reset.TRooms == 0,
      "dungeon reset must clear records, selector, and layout progress");
  }

  private static void TestDungeonRewardBoundary()
  {
    DungeonSpecialRewardGenerationComponent component =
      new(generationId: 802);
    DungeonSpecialRewardGenerationSystem.Commit(
      component,
      new DungeonSpecialRewardGenerationSnapshot(802, true, true));

    DungeonSpecialRewardGenerationSnapshot snapshot =
      DungeonSpecialRewardGenerationQuery.Snapshot(component);
    Require(snapshot.GeneratedShadowKey && snapshot.GeneratedRamRune, "reward flags must commit together");

    DungeonSpecialRewardGenerationResetSystem.Reset(component);
    snapshot = DungeonSpecialRewardGenerationQuery.Snapshot(component);
    Require(!snapshot.GeneratedShadowKey && !snapshot.GeneratedRamRune, "reward reset must clear both flags");

    RequireThrows<ArgumentException>(
      () => DungeonSpecialRewardGenerationSystem.Commit(
        component,
        new DungeonSpecialRewardGenerationSnapshot(803, true, false)),
      "reward commit must reject another generation");
  }

  private static void TestFloatingIslandBoundary()
  {
    FloatingIslandPlacementStateComponent component =
      new(generationId: 803, skyLakes: 3, skyIslandHouseCount: 4);
    FloatingIslandHouseSnapshot first = new(803, true, 10, 20, 1);
    FloatingIslandHouseSnapshot second = new(803, false, 30, 40, 2);

    Require(FloatingIslandPlacementSystem.TryAppend(component, first), "first island house must append");
    Require(FloatingIslandPlacementSystem.TryAppend(component, second), "second island house must append");
    FloatingIslandPlacementSnapshot snapshot = FloatingIslandPlacementQuery.Snapshot(component);
    Require(snapshot.Count == 2 && snapshot.Houses[1].Style == 2, "island metadata must preserve pairing and order");
    Require(((IList<FloatingIslandHouseSnapshot>)snapshot.Houses).IsReadOnly, "island snapshot must be read-only");

    FloatingIslandPlacementSystem.Clear(component);
    Require(FloatingIslandPlacementQuery.Snapshot(component).Count == 0, "island clear must remove used entries");

    for (int index = 0; index < FloatingIslandPlacementStateComponent.Capacity; index++)
    {
      Require(
        FloatingIslandPlacementSystem.TryAppend(
          component,
          new FloatingIslandHouseSnapshot(803, index % 2 == 0, index, index + 1, index % 4)),
        "island capacity entries must append");
    }

    Require(
      !FloatingIslandPlacementSystem.TryAppend(component, new FloatingIslandHouseSnapshot(803, false, 0, 0, 0)),
      "island append beyond capacity must reject");

    FloatingIslandPlacementResetSystem.ResetProgress(component);
    snapshot = FloatingIslandPlacementQuery.Snapshot(component);
    Require(
      snapshot.Count == 0 && snapshot.SkyIslandHouseCount == 0 && snapshot.SkyLakes == 3,
      "island progress reset must clear used count while preserving sky-lake target");

    RequireThrows<ArgumentException>(
      () => FloatingIslandPlacementSystem.TryAppend(
        component,
        new FloatingIslandHouseSnapshot(804, false, 0, 0, 0)),
      "island metadata must reject another generation");
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
