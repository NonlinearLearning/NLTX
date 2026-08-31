using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleDefinition style = new(1, null, 2, 3, 4, 5, 6);
DungeonStyleLookupEntry entry = new(style);
DungeonLayoutProviderSettingsSnapshot settings = new(entry);
if (settings.Style != entry ||
    settings.Style.Style.BrickTileType != style.BrickTileType ||
    settings.Style.Style.BrickWallType != style.BrickWallType)
{
  throw new InvalidOperationException("Dungeon layout provider settings diverged.");
}

AssertThrows<ArgumentNullException>(() =>
  new DungeonLayoutProviderSettingsSnapshot(null!));

Console.WriteLine("PASS: Dungeon layout provider settings contract");

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
