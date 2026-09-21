using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
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
      TestStructureStateAndCommit();
      TestLarvaCapacityAndSnapshotIsolation();
      TestUndergroundDesertReset();
      TestLarvaPlacementProjection();
      TestLarvaTileSolidityProjection();
      TestReservationGate();
      TestUndergroundDesertBoundsQuery();
      Console.WriteLine("C06 underground-desert focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestStructureStateAndCommit()
  {
    UndergroundDesertStructureComponent component = new(
      generationId: 101,
      undergroundDesertLocation: new UndergroundDesertRectangle(10, 20, 30, 40),
      undergroundDesertHiveLocation: new UndergroundDesertRectangle(15, 25, 10, 20),
      desertHiveHigh: 300,
      desertHiveLow: 100,
      desertHiveLeft: 12,
      desertHiveRight: 48);

    UndergroundDesertStructureSnapshot initial =
      UndergroundDesertStructureQuery.Snapshot(component);
    Require(initial.GenerationId == 101, "desert generation identity");
    Require(initial.UndergroundDesertLocation == new UndergroundDesertRectangle(10, 20, 30, 40), "desert location");
    Require(initial.UndergroundDesertHiveLocation == new UndergroundDesertRectangle(15, 25, 10, 20), "hive location");
    Require(initial.DesertHiveHigh == 300, "hive high");
    Require(initial.DesertHiveLow == 100, "hive low");
    Require(initial.DesertHiveLeft == 12, "hive left");
    Require(initial.DesertHiveRight == 48, "hive right");

    UndergroundDesertStructureSnapshot replacement = new(
      GenerationId: 101,
      UndergroundDesertLocation: new UndergroundDesertRectangle(-1, -2, 3, 4),
      UndergroundDesertHiveLocation: new UndergroundDesertRectangle(5, 6, 7, 8),
      DesertHiveHigh: -9,
      DesertHiveLow: -10,
      DesertHiveLeft: -11,
      DesertHiveRight: -12);
    UndergroundDesertStructureSystem.Commit(component, replacement);
    Require(
      UndergroundDesertStructureQuery.Snapshot(component) == replacement,
      "desert layout commit replaces every field");
    RequireThrows<ArgumentException>(
      () => UndergroundDesertStructureSystem.Commit(
        component,
        replacement with { GenerationId = 102 }),
      "desert layout commit must reject a stale generation");
    Require(
      UndergroundDesertStructureQuery.Snapshot(component) == replacement,
      "stale desert layout commit preserves state");
  }

  private static void TestLarvaCapacityAndSnapshotIsolation()
  {
    UndergroundDesertLarvaPlacementComponent component =
      new(generationId: 103);
    TilePosition first = new(10, 20);
    TilePosition second = new(30, 40);
    Require(
      UndergroundDesertLarvaPlacementSystem.TryAppend(component, first),
      "first larva append");
    Require(
      UndergroundDesertLarvaPlacementSystem.TryAppend(component, second),
      "second larva append");

    UndergroundDesertLarvaPlacementSnapshot snapshot =
      UndergroundDesertLarvaPlacementQuery.Snapshot(component);
    Require(snapshot.GenerationId == 103, "larva generation identity");
    Require(snapshot.Count == 2, "larva count");
    Require(snapshot.Positions[0] == first, "first larva position");
    Require(snapshot.Positions[1] == second, "second larva position");
    Require(
      ((IList<TilePosition>)snapshot.Positions).IsReadOnly,
      "larva snapshot must be read-only");

    for (int index = component.Count;
      index < UndergroundDesertLarvaPlacementComponent.Capacity;
      index++)
    {
      Require(
        UndergroundDesertLarvaPlacementSystem.TryAppend(
          component,
          new TilePosition(index, index + 100)),
        "larva append within capacity");
    }

    Require(
      !UndergroundDesertLarvaPlacementSystem.TryAppend(
        component,
        new TilePosition(999, 1099)),
      "larva capacity rejects overflow");
    UndergroundDesertLarvaPlacementSystem.Clear(component);
    Require(component.Count == 0, "larva clear resets used count");
    Require(
      snapshot.Count == 2 && snapshot.Positions[0] == first,
      "larva snapshot is isolated from clear");
  }

  private static void TestUndergroundDesertReset()
  {
    UndergroundDesertStructureComponent structure = new(
      generationId: 104,
      undergroundDesertLocation: new UndergroundDesertRectangle(10, 20, 30, 40),
      undergroundDesertHiveLocation: new UndergroundDesertRectangle(15, 25, 10, 20),
      desertHiveHigh: 300,
      desertHiveLow: 100,
      desertHiveLeft: 12,
      desertHiveRight: 48);
    UndergroundDesertLarvaPlacementComponent larva = new(generationId: 104);
    Require(
      UndergroundDesertLarvaPlacementSystem.TryAppend(
        larva,
        new TilePosition(50, 60)),
      "reset fixture larva append");

    UndergroundDesertResetInput input = new(worldWidth: 4200, worldHeight: 1200);
    UndergroundDesertResetSystem.Reset(structure, larva, in input);

    UndergroundDesertStructureSnapshot structureSnapshot =
      UndergroundDesertStructureQuery.Snapshot(structure);
    Require(
      structureSnapshot.UndergroundDesertLocation.IsEmpty &&
        structureSnapshot.UndergroundDesertHiveLocation.IsEmpty,
      "reset must restore empty underground-desert rectangles");
    Require(
      structureSnapshot.DesertHiveHigh == 1200 &&
        structureSnapshot.DesertHiveLow == 0 &&
        structureSnapshot.DesertHiveLeft == 4200 &&
        structureSnapshot.DesertHiveRight == 0,
      "reset must restore Version4 desert-hive dimension sentinels");

    UndergroundDesertLarvaPlacementSnapshot larvaSnapshot =
      UndergroundDesertLarvaPlacementQuery.Snapshot(larva);
    Require(larvaSnapshot.Count == 0, "reset must clear larva count");
    Require(larvaSnapshot.Positions.Count == 0, "reset must clear larva positions");
    Require(
      UndergroundDesertLarvaPlacementSystem.TryAppend(
        larva,
        new TilePosition(70, 80)),
      "reset must leave the larva owner reusable");
    Require(
      UndergroundDesertLarvaPlacementQuery.Snapshot(larva).Positions[0] ==
        new TilePosition(70, 80),
      "reset must not retain prior-generation larva coordinates");
  }

  private static void TestLarvaPlacementProjection()
  {
    TilePosition anchor = new(100, 200);
    UndergroundDesertLarvaPlacementSnapshot snapshot = new(
      GenerationId: 105,
      Count: 1,
      Positions: new[] { anchor });

    UndergroundDesertLarvaPlacementCommand[] commands =
      UndergroundDesertLarvaPlacementProjection.CreateCommands(in snapshot);
    Require(commands.Length == 1, "one larva anchor produces one placement command");

    UndergroundDesertLarvaPlacementCommand command = commands[0];
    Require(command.GenerationId == 105, "larva projection generation identity");
    Require(command.Anchor == anchor, "larva projection anchor");
    Require(command.Mutations.Count == 13, "larva projection footprint operation count");
    Require(
      ((IList<UndergroundDesertLarvaTileMutation>)command.Mutations).IsReadOnly,
      "larva projection mutations must be read-only");

    UndergroundDesertLarvaTileMutation first = command.Mutations[0];
    Require(
      first.Kind == UndergroundDesertLarvaTileMutation.OperationKind.DeactivateTile &&
        first.Target == new TilePosition(99, 198),
      "larva projection preserves first clear operation");

    UndergroundDesertLarvaTileMutation support = command.Mutations[3];
    Require(
      support.Kind == UndergroundDesertLarvaTileMutation.OperationKind.ConfigureFoundationTile &&
        support.Target == new TilePosition(99, 201) &&
        support.IsActive &&
        support.TileType == 225 &&
        support.Slope == 0 &&
        !support.HalfBrick,
      "larva projection preserves support tile operation");

    UndergroundDesertLarvaTileMutation placeObject = command.Mutations[12];
    Require(
      placeObject.Kind == UndergroundDesertLarvaTileMutation.OperationKind.PlaceLarvaObject &&
        placeObject.Target == anchor &&
        placeObject.IsActive &&
        placeObject.TileType == 231 &&
        placeObject.Mute,
      "larva projection preserves larva object placement");
  }

  private static void TestReservationGate()
  {
    InMemoryStructureReservationAdapter reservations =
      new();
    StructureReservationIntent reservation = new(
      GenerationId: 106,
      ReservationId: "underground-desert",
      Bounds: new WorldGenerationRectangle(20, 30, 40, 50));
    UndergroundDesertLarvaPlacementSnapshot snapshot = new(
      GenerationId: 106,
      Count: 1,
      Positions: new[] { new TilePosition(30, 40) });

    UndergroundDesertPlacementCommitResult accepted =
      UndergroundDesertPlacementCommitSystem.ReserveAndProjectLarvaCommands(
        reservations,
        in reservation,
        in snapshot);
    Require(accepted.ReservationAccepted, "reservation gate accepts the first reservation");
    Require(accepted.LarvaCommandCount == 1, "accepted reservation publishes larva commands");
    Require(
      ((IList<UndergroundDesertLarvaPlacementCommand>)accepted.LarvaCommands).IsReadOnly,
      "accepted larva commands must be read-only");

    UndergroundDesertPlacementCommitResult rejected =
      UndergroundDesertPlacementCommitSystem.ReserveAndProjectLarvaCommands(
        reservations,
        in reservation,
        in snapshot);
    Require(!rejected.ReservationAccepted, "overlapping reservation is rejected");
    Require(rejected.LarvaCommandCount == 0, "rejected reservation publishes no larva commands");

    UndergroundDesertLarvaPlacementSnapshot staleSnapshot = snapshot with
    {
      GenerationId = 107,
    };
    RequireThrows<ArgumentException>(
      () => UndergroundDesertPlacementCommitSystem.ReserveAndProjectLarvaCommands(
        reservations,
        in reservation,
        in staleSnapshot),
      "reservation gate must reject mixed-generation inputs before reservation");
    Require(
      reservations.CreateSnapshot().Count == 1,
      "mixed-generation rejection must not create a reservation");
  }

  private static void TestLarvaTileSolidityProjection()
  {
    IReadOnlyList<UndergroundDesertLarvaTileSolidityCommand> commands =
      UndergroundDesertLarvaTileSolidityProjection.CreateCommands(
        generationId: 108);
    Require(commands.Count == 3, "larva pass projects three tile-solidity updates");
    Require(
      ((IList<UndergroundDesertLarvaTileSolidityCommand>)commands).IsReadOnly,
      "larva tile-solidity commands must be read-only");

    UndergroundDesertLarvaTileSolidityCommand before = commands[0];
    Require(
      before.GenerationId == 108 &&
        before.Timing == UndergroundDesertLarvaTileSolidityCommand.Phase.BeforeLarvaPlacement &&
        before.TileTypeId == 229 &&
        before.Solid,
      "tile type 229 must become solid before larva placement");

    UndergroundDesertLarvaTileSolidityCommand afterFirst = commands[1];
    UndergroundDesertLarvaTileSolidityCommand afterSecond = commands[2];
    Require(
      afterFirst.Timing == UndergroundDesertLarvaTileSolidityCommand.Phase.AfterLarvaPlacement &&
        afterFirst.TileTypeId == 232 &&
        afterFirst.Solid &&
        afterSecond.Timing == UndergroundDesertLarvaTileSolidityCommand.Phase.AfterLarvaPlacement &&
        afterSecond.TileTypeId == 162 &&
        afterSecond.Solid,
      "tile types 232 and 162 must become solid after larva placement");

    foreach (UndergroundDesertLarvaTileSolidityCommand command in commands)
    {
      command.Validate();
    }
  }

  private static void TestUndergroundDesertBoundsQuery()
  {
    UndergroundDesertStructureComponent component = new(
      generationId: 109,
      undergroundDesertLocation: new UndergroundDesertRectangle(10, 20, 30, 40),
      undergroundDesertHiveLocation: new UndergroundDesertRectangle(15, 25, 10, 20));

    Require(
      UndergroundDesertStructureQuery.ContainsDesertTile(
        component,
        new TilePosition(10, 20)),
      "desert bounds include the top-left edge");
    Require(
      !UndergroundDesertStructureQuery.ContainsDesertTile(
        component,
        new TilePosition(40, 60)),
      "desert bounds exclude the right-bottom edge");
    Require(
      UndergroundDesertStructureQuery.ContainsHiveTile(
        component,
        new TilePosition(15, 25)),
      "hive bounds include the top-left edge");
    Require(
      UndergroundDesertStructureQuery.IsHiveWithinDesert(component),
      "hive rectangle is contained by the desert rectangle");
    Require(
      UndergroundDesertStructureQuery.IntersectsDesertArea(
        component,
        new UndergroundDesertRectangle(39, 59, 2, 2)),
      "desert query detects a positive-area overlap");
    Require(
      !UndergroundDesertStructureQuery.IntersectsDesertArea(
        component,
        new UndergroundDesertRectangle(40, 20, 4, 4)),
      "desert query rejects touching rectangles");
    Require(
      !UndergroundDesertStructureQuery.IntersectsHiveArea(
        component,
        new UndergroundDesertRectangle(25, 45, 2, 2)),
      "hive query rejects touching rectangles");
    Require(
      !new UndergroundDesertRectangle(0, 0, 0, 10).Contains(
        new TilePosition(0, 0)),
      "zero-width rectangles contain no tile");
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
