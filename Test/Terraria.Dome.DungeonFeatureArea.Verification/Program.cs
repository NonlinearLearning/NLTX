using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonFeatureArea area = DungeonFeatureArea.FromCenterAndFluff(10, 20, 2);
if (!area.HasHitbox || area.Left != 8 || area.Top != 18 || area.Right != 12 || area.Bottom != 22 ||
    area.Width != 5 || area.Height != 5 || area.CellCount != 25 ||
    !area.Contains(8, 18) || !area.Contains(12, 22) || area.Contains(13, 22))
{
  throw new InvalidOperationException("Dungeon feature area footprint diverged.");
}

DungeonFeatureArea empty = new(5, 5, 4, 4);
if (empty.HasHitbox || empty.CellCount != 0 || empty.Contains(5, 5))
{
  throw new InvalidOperationException("Empty dungeon feature area diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => DungeonFeatureArea.FromCenterAndFluff(0, 0, -1));
AssertThrows<OverflowException>(() => DungeonFeatureArea.FromCenterAndFluff(int.MaxValue, 0, 1));

Console.WriteLine("PASS: Dungeon feature area contract");

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
