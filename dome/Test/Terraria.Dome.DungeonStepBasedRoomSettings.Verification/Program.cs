using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStepBasedRoomSettings settings = new(10, 4, 0.75);
if (settings.OverrideStrength != 10 ||
    settings.OverrideSteps != 4 ||
    settings.OverrideInteriorToExteriorRatio != 0.75 ||
    settings.GetBoundingRadius() != DungeonRoomBoundingRadiusQuery.StepBased(10, 4))
{
  throw new InvalidOperationException("Step-based dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonStepBasedRoomSettings(-1, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonStepBasedRoomSettings(0, -1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonStepBasedRoomSettings(0, 0, double.NaN));

Console.WriteLine("PASS: Step-based dungeon room settings contract");

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
