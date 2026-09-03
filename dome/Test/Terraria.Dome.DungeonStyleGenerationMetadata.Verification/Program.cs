using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleGenerationMetadata defaults = DungeonStyleGenerationMetadata.Default;
if (defaults.UnbreakableWallProgressionTier != -1 ||
    defaults.LiquidType != -1 ||
    defaults.EdgeDither ||
    defaults.BiomeRoomType != DungeonRoomType.BiomeStructured)
{
  throw new InvalidOperationException("Dungeon style scalar defaults diverged.");
}

DungeonStyleGenerationMetadata configured = new(2, 3, true, DungeonRoomType.BiomeRugged);
if (configured.UnbreakableWallProgressionTier != 2 ||
    configured.LiquidType != 3 ||
    !configured.EdgeDither ||
    configured.BiomeRoomType != DungeonRoomType.BiomeRugged)
{
  throw new InvalidOperationException("Dungeon style scalar metadata did not preserve values.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonStyleGenerationMetadata(-2, -1, false, DungeonRoomType.BiomeStructured));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonStyleGenerationMetadata(-1, -2, false, DungeonRoomType.BiomeStructured));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonStyleGenerationMetadata(-1, -1, false, (DungeonRoomType)99));

Console.WriteLine("PASS: Dungeon style generation metadata contract");

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
