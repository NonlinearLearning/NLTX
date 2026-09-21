using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestBoundarySnapshotAndQuery();
      TestBoundaryCommitPreservesRawValues();
      TestBoundaryCalculationPreservesSourceBranches();
      TestBoundaryCalculationSystemCommitsCalculatedValues();
      TestBoundaryCalculationRejectsOutOfRangeRolls();
      TestLegacyCalculationParityMatrix();
      Console.WriteLine("C04 beach-boundary focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestBoundarySnapshotAndQuery()
  {
    BeachBoundaryComponent component = new(
      generationId: 84,
      leftBeachEnd: 10,
      rightBeachStart: 90,
      beachBordersWidth: 12,
      beachSandRandomCenter: 20,
      beachSandRandomWidthRange: 8,
      beachSandDungeonExtraWidth: 4,
      beachSandJungleExtraWidth: 6,
      shellStartXLeft: 11,
      shellStartYLeft: 22,
      shellStartXRight: 88,
      shellStartYRight: 77,
      oceanWaterStartRandomMin: 33);

    BeachBoundarySnapshot snapshot = BeachBoundaryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 84, "boundary generation identity");
    Require(snapshot.LeftBeachEnd == 10, "left beach boundary");
    Require(snapshot.RightBeachStart == 90, "right beach boundary");
    Require(snapshot.BeachBordersWidth == 12, "beach border width");
    Require(snapshot.BeachSandRandomCenter == 20, "beach random center");
    Require(snapshot.BeachSandRandomWidthRange == 8, "beach random width range");
    Require(snapshot.BeachSandDungeonExtraWidth == 4, "dungeon extra width");
    Require(snapshot.BeachSandJungleExtraWidth == 6, "jungle extra width");
    Require(snapshot.ShellStartXLeft == 11, "left shell x");
    Require(snapshot.ShellStartYLeft == 22, "left shell y");
    Require(snapshot.ShellStartXRight == 88, "right shell x");
    Require(snapshot.ShellStartYRight == 77, "right shell y");
    Require(snapshot.OceanWaterStartRandomMin == 33, "ocean random lower bound");
  }

  private static void TestBoundaryCommitPreservesRawValues()
  {
    BeachBoundaryComponent component = new(generationId: 85);
    BeachBoundarySnapshot rawValues = new(
      GenerationId: 85,
      LeftBeachEnd: 900,
      RightBeachStart: 100,
      BeachBordersWidth: -1,
      BeachSandRandomCenter: -2,
      BeachSandRandomWidthRange: -3,
      BeachSandDungeonExtraWidth: -4,
      BeachSandJungleExtraWidth: -5,
      ShellStartXLeft: 600,
      ShellStartYLeft: 601,
      ShellStartXRight: 200,
      ShellStartYRight: 201,
      OceanWaterStartRandomMin: -6);

    BeachBoundarySystem.Commit(component, rawValues);
    BeachBoundarySnapshot committed = component.CreateSnapshot();
    Require(committed == rawValues, "boundary commit preserves raw source values");
    Require(
      BeachBoundaryQuery.Snapshot(component) == rawValues,
      "boundary query returns committed values");
    RequireThrows<ArgumentException>(
      () => BeachBoundarySystem.Commit(
        component,
        rawValues with { GenerationId = 86 }),
      "boundary commit must reject a stale generation");
  }

  private static void TestBoundaryCalculationPreservesSourceBranches()
  {
    BeachBoundaryCalculationInput leftDungeonInput = CreateCalculationInput(
      dungeonSide: -1,
      leftBeachRandomRoll: 300,
      rightBeachRandomRoll: 339);
    BeachBoundarySnapshot leftDungeonResult =
      BeachBoundaryCalculationQuery.Calculate(in leftDungeonInput);

    Require(leftDungeonResult.LeftBeachEnd == 320, "left-side jungle width adjustment");
    Require(leftDungeonResult.RightBeachStart == 621, "left-side dungeon width adjustment");
    Require(leftDungeonResult.ShellStartXLeft == 11, "calculation preserves left shell x");
    Require(leftDungeonResult.OceanWaterStartRandomMin == 220, "calculation preserves water bound");

    BeachBoundaryCalculationInput rightDungeonInput = CreateCalculationInput(
      dungeonSide: 1,
      leftBeachRandomRoll: 300,
      rightBeachRandomRoll: 339);
    BeachBoundarySnapshot rightDungeonResult =
      BeachBoundaryCalculationQuery.Calculate(in rightDungeonInput);

    Require(rightDungeonResult.LeftBeachEnd == 340, "right-side dungeon width adjustment");
    Require(rightDungeonResult.RightBeachStart == 641, "right-side jungle width adjustment");
  }

  private static void TestBoundaryCalculationSystemCommitsCalculatedValues()
  {
    BeachBoundaryComponent component = new(generationId: 86);
    BeachBoundaryCalculationInput input = CreateCalculationInput(
      dungeonSide: 1,
      leftBeachRandomRoll: -100,
      rightBeachRandomRoll: -200,
      isTenthAnniversaryWorld: true,
      isRemixWorld: false);

    BeachBoundarySnapshot result =
      BeachBoundaryCalculationSystem.CalculateAndCommit(component, in input);

    Require(result.GenerationId == 86, "calculation commit generation identity");
    Require(result.LeftBeachEnd == 380, "anniversary fixed left boundary");
    Require(result.RightBeachStart == 640, "anniversary fixed right boundary");
    Require(component.CreateSnapshot() == result, "calculation system commits snapshot");
  }

  private static void TestBoundaryCalculationRejectsOutOfRangeRolls()
  {
    BeachBoundaryCalculationInput input = CreateCalculationInput(
      dungeonSide: -1,
      leftBeachRandomRoll: 299,
      rightBeachRandomRoll: 339);

    RequireThrows<ArgumentOutOfRangeException>(
      () => BeachBoundaryCalculationQuery.Calculate(in input),
      "left beach random roll lower bound");
  }

  private static void TestLegacyCalculationParityMatrix()
  {
    BeachBoundaryCalculationInput[] inputs =
    [
      CreateCalculationInput(
        dungeonSide: -1,
        leftBeachRandomRoll: 300,
        rightBeachRandomRoll: 339),
      CreateCalculationInput(
        dungeonSide: 1,
        leftBeachRandomRoll: 339,
        rightBeachRandomRoll: 300),
      CreateCalculationInput(
        dungeonSide: -1,
        leftBeachRandomRoll: 319,
        rightBeachRandomRoll: 320,
        isTenthAnniversaryWorld: true,
        isRemixWorld: true),
      CreateCalculationInput(
        dungeonSide: 1,
        leftBeachRandomRoll: -100,
        rightBeachRandomRoll: 900,
        isTenthAnniversaryWorld: true,
        isRemixWorld: false)
    ];

    foreach (BeachBoundaryCalculationInput input in inputs)
    {
      BeachBoundarySnapshot actual =
        BeachBoundaryCalculationQuery.Calculate(in input);
      BeachBoundarySnapshot expected = CalculateLegacyBoundary(in input);
      Require(
        actual == expected,
        $"legacy boundary parity for dungeon side {input.DungeonSide}");
    }
  }

  private static BeachBoundarySnapshot CalculateLegacyBoundary(
    in BeachBoundaryCalculationInput input)
  {
    bool useTenthAnniversaryFixedBoundary =
      input.IsTenthAnniversaryWorld && !input.IsRemixWorld;

    int leftBeachEnd = useTenthAnniversaryFixedBoundary
      ? unchecked(input.BeachSandRandomCenter + input.BeachSandRandomWidthRange)
      : input.LeftBeachRandomRoll;
    if (input.DungeonSide == 1)
    {
      leftBeachEnd = unchecked(
        leftBeachEnd + input.BeachSandDungeonExtraWidth);
    }
    else
    {
      leftBeachEnd = unchecked(
        leftBeachEnd + input.BeachSandJungleExtraWidth);
    }

    int rightBeachStart = useTenthAnniversaryFixedBoundary
      ? unchecked(
        input.MaxTilesX -
        (input.BeachSandRandomCenter + input.BeachSandRandomWidthRange))
      : unchecked(input.MaxTilesX - input.RightBeachRandomRoll);
    if (input.DungeonSide == -1)
    {
      rightBeachStart = unchecked(
        rightBeachStart - input.BeachSandDungeonExtraWidth);
    }
    else
    {
      rightBeachStart = unchecked(
        rightBeachStart - input.BeachSandJungleExtraWidth);
    }

    return new BeachBoundarySnapshot(
      input.GenerationId,
      leftBeachEnd,
      rightBeachStart,
      input.BeachBordersWidth,
      input.BeachSandRandomCenter,
      input.BeachSandRandomWidthRange,
      input.BeachSandDungeonExtraWidth,
      input.BeachSandJungleExtraWidth,
      input.ShellStartXLeft,
      input.ShellStartYLeft,
      input.ShellStartXRight,
      input.ShellStartYRight,
      input.OceanWaterStartRandomMin);
  }

  private static BeachBoundaryCalculationInput CreateCalculationInput(
    short dungeonSide,
    int leftBeachRandomRoll,
    int rightBeachRandomRoll,
    bool isTenthAnniversaryWorld = false,
    bool isRemixWorld = false)
  {
    return new BeachBoundaryCalculationInput(
      GenerationId: 86,
      MaxTilesX: 1000,
      BeachBordersWidth: 275,
      BeachSandRandomCenter: 320,
      BeachSandRandomWidthRange: 20,
      BeachSandDungeonExtraWidth: 40,
      BeachSandJungleExtraWidth: 20,
      ShellStartXLeft: 11,
      ShellStartYLeft: 22,
      ShellStartXRight: 988,
      ShellStartYRight: 77,
      OceanWaterStartRandomMin: 220,
      DungeonSide: dungeonSide,
      IsTenthAnniversaryWorld: isTenthAnniversaryWorld,
      IsRemixWorld: isRemixWorld,
      LeftBeachRandomRoll: leftBeachRandomRoll,
      RightBeachRandomRoll: rightBeachRandomRoll);
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
