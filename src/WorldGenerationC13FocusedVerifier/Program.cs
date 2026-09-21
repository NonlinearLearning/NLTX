using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestSelectionAndCurrentRecordQuery();
      TestUpperIndexFailureIsPreserved();
      TestDungeonControlLineForwarding();
      Console.WriteLine("C13 dungeon derived-properties focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestSelectionAndCurrentRecordQuery()
  {
    DungeonLayoutControlComponent component =
      new(generationId: 71);
    DungeonLayoutControlSystem.Commit(
      component,
      new DungeonLayoutSnapshot(
        GenerationId: 71,
        TLeft: 1,
        TRight: 2,
        TTop: 3,
        TBottom: 4,
        TRooms: 5,
        LAltarX: 6,
        LAltarY: 7,
        Records: new[]
        {
          new DungeonRecordSnapshot(101),
          new DungeonRecordSnapshot(202)
        },
        CurrentDungeon: 0));

    DungeonSelectionControlSystem.Select(component, -3);
    Require(component.CurrentDungeon == 0, "negative dungeon selection clamps to zero");

    DungeonSelectionControlSystem.Select(component, 1);
    DungeonLayoutSnapshot snapshot = DungeonLayoutQuery.Snapshot(component);
    Require(
      DungeonDerivedPropertiesQuery.CurrentDungeon(snapshot) == 1,
      "positive dungeon selection is preserved");
    Require(
      DungeonDerivedPropertiesQuery.CurrentDungeonGenVars(snapshot).RecordId == 202,
      "current dungeon query returns the selected C08 record");
    Require(
      LegacyDungeonDerivedPropertiesAdapter.GetCurrentDungeonGenVars(component).RecordId == 202,
      "legacy adapter reads the same C08 record authority");
    Require(
      ((IList<DungeonRecordSnapshot>)snapshot.Records).IsReadOnly,
      "dungeon snapshot records are read-only");
  }

  private static void TestUpperIndexFailureIsPreserved()
  {
    DungeonLayoutControlComponent component =
      new(generationId: 72);
    DungeonLayoutControlSystem.Commit(
      component,
      new DungeonLayoutSnapshot(
        GenerationId: 72,
        TLeft: 0,
        TRight: 0,
        TTop: 0,
        TBottom: 0,
        TRooms: 0,
        LAltarX: 0,
        LAltarY: 0,
        Records: new[] { new DungeonRecordSnapshot(303) },
        CurrentDungeon: 0));

    DungeonSelectionControlSystem.Select(component, 4);
    RequireThrows<ArgumentOutOfRangeException>(
      () => LegacyDungeonDerivedPropertiesAdapter.GetCurrentDungeonGenVars(component),
      "upper dungeon selection must preserve direct index failure");
  }

  private static void TestDungeonControlLineForwarding()
  {
    TestControlLine controlLine = new(0.25);
    DungeonControlLineAdapter adapter = new(controlLine);

    Require(
      DungeonDerivedPropertiesQuery.DualDungeonNormalizedDistanceSafeFromDither(adapter) ==
        0.25,
      "dungeon control-line read is forwarded");

    adapter.NormalizedDistanceSafeFromDither = -4.5;
    Require(
      controlLine.NormalizedDistanceSafeFromDither == -4.5 &&
        DungeonDerivedPropertiesQuery.DualDungeonNormalizedDistanceSafeFromDither(adapter) == -4.5,
      "dungeon control-line write is forwarded without clamping or caching");
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

  private sealed class TestControlLine : IDungeonControlLinePort
  {
    public TestControlLine(double normalizedDistanceSafeFromDither)
    {
      NormalizedDistanceSafeFromDither = normalizedDistanceSafeFromDither;
    }

    public double NormalizedDistanceSafeFromDither { get; set; }
  }
}
