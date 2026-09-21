using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonSizeScalars scalars = new(
  hallStrengthScalar: 0.8,
  hallStepScalar: 1.2,
  roomStrengthScalar: 1.5,
  roomStepScalar: 0.5);

if (scalars.HallSizeScalar != 1.0 || scalars.RoomSizeScalar != 1.0)
{
  throw new InvalidOperationException("Dungeon size scalar averages diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonSizeScalars(
  double.NaN,
  1.0,
  1.0,
  1.0));

Console.WriteLine("PASS: Dungeon size scalar contract");

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
