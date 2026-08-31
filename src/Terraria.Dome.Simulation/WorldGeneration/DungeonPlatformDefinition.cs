using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonPlatformDefinition
{
  public DungeonPlatformDefinition(
    double placeBooksChance,
    double placePotsChance,
    double placeWaterCandlesChance,
    double placePotionBottlesChance)
  {
    if (!double.IsFinite(placeBooksChance) ||
        !double.IsFinite(placePotsChance) ||
        !double.IsFinite(placeWaterCandlesChance) ||
        !double.IsFinite(placePotionBottlesChance))
    {
      throw new ArgumentOutOfRangeException("chance");
    }

    PlaceBooksChance = placeBooksChance;
    PlacePotsChance = placePotsChance;
    PlaceWaterCandlesChance = placeWaterCandlesChance;
    PlacePotionBottlesChance = placePotionBottlesChance;
  }

  public double PlaceBooksChance { get; }

  public double PlacePotsChance { get; }

  public double PlaceWaterCandlesChance { get; }

  public double PlacePotionBottlesChance { get; }

  public bool IsAShelf
  {
    get
    {
      if (!(PlaceBooksChance > 0.0) &&
          !(PlacePotsChance > 0.0) &&
          !(PlaceWaterCandlesChance > 0.0))
      {
        return PlacePotionBottlesChance > 0.0;
      }

      return true;
    }
  }
}
