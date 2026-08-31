using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (DungeonBiomeRoomSizeQuery.GetInnerSize(4200, DungeonGenerationStyleId.Dungeon) != 32 ||
    DungeonBiomeRoomSizeQuery.GetInnerSize(4200, DungeonGenerationStyleId.Temple) != 50 ||
    DungeonBiomeRoomSizeQuery.GetOuterSize(4200, DungeonGenerationStyleId.Dungeon) != 40 ||
    DungeonBiomeRoomSizeQuery.GetInnerSize(2100, DungeonGenerationStyleId.Dungeon) != 16)
{
  throw new InvalidOperationException("Biome dungeon room size formula diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => DungeonBiomeRoomSizeQuery.GetInnerSize(0, 0));
AssertThrows<ArgumentOutOfRangeException>(() => DungeonBiomeRoomSizeQuery.GetInnerSize(4200, -1));

Console.WriteLine("PASS: Biome dungeon room size contract");

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
