using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonPlatformPlacementIntent intent = new(
  positionX: 7,
  positionY: 9,
  overrideStyle: 2,
  overrideMaxLengthAllowed: 18,
  overrideHeightFluff: 3,
  inAHallway: true,
  forcePlacement: true,
  skipOtherPlatformsCheck: true,
  skipSpaceCheck: false,
  placeBooksChance: 0.25,
  noWaterbolt: true,
  placePotsChance: 0.5,
  placeWaterCandlesChance: 0.75,
  placePotionBottlesChance: 0,
  isAShelf: true);

if (intent.PositionX != 7 || intent.PositionY != 9 || intent.OverrideStyle != 2 ||
    intent.OverrideMaxLengthAllowed != 18 || intent.OverrideHeightFluff != 3 ||
    !intent.InAHallway || !intent.ForcePlacement || !intent.SkipOtherPlatformsCheck ||
    intent.SkipSpaceCheck || intent.PlaceBooksChance != 0.25 || !intent.NoWaterbolt ||
    intent.PlacePotsChance != 0.5 || intent.PlaceWaterCandlesChance != 0.75 ||
    intent.PlacePotionBottlesChance != 0 || !intent.IsAShelf)
{
  throw new InvalidOperationException("Dungeon platform placement intent diverged.");
}

Console.WriteLine("PASS: Dungeon platform placement intent contract");
