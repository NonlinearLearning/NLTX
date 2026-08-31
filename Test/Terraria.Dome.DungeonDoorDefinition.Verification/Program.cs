using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonDoorDefinition definition = new(
  positionX: 12,
  positionY: 34,
  overrideBrickTileType: 100,
  overrideBrickWallType: 200,
  overrideStyle: 3,
  direction: -1,
  inAHallway: true,
  overrideWidthFluff: 4,
  skipOtherDoorsCheck: true,
  skipSpaceCheck: false,
  alwaysClearArea: true);

if (definition.PositionX != 12 ||
    definition.PositionY != 34 ||
    definition.OverrideBrickTileType != 100 ||
    definition.OverrideBrickWallType != 200 ||
    definition.OverrideStyle != 3 ||
    definition.Direction != -1 ||
    !definition.InAHallway ||
    definition.OverrideWidthFluff != 4 ||
    !definition.SkipOtherDoorsCheck ||
    definition.SkipSpaceCheck ||
    !definition.AlwaysClearArea)
{
  throw new InvalidOperationException("Dungeon door definition fields diverged.");
}

DungeonDoorDefinition nullable = new(
  0,
  0,
  null,
  null,
  null,
  0,
  false,
  null,
  false,
  false,
  false);
if (nullable.OverrideStyle.HasValue || nullable.OverrideWidthFluff.HasValue)
{
  throw new InvalidOperationException("Dungeon door nullable overrides diverged.");
}

Console.WriteLine("PASS: Dungeon door definition contract");
