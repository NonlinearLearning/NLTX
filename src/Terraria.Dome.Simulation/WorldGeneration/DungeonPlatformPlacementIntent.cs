namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonPlatformPlacementIntent
{
  public DungeonPlatformPlacementIntent(
    int positionX,
    int positionY,
    int? overrideStyle,
    int overrideMaxLengthAllowed,
    int? overrideHeightFluff,
    bool inAHallway,
    bool forcePlacement,
    bool skipOtherPlatformsCheck,
    bool skipSpaceCheck,
    double placeBooksChance,
    bool noWaterbolt,
    double placePotsChance,
    double placeWaterCandlesChance,
    double placePotionBottlesChance,
    bool isAShelf)
  {
    PositionX = positionX;
    PositionY = positionY;
    OverrideStyle = overrideStyle;
    OverrideMaxLengthAllowed = overrideMaxLengthAllowed;
    OverrideHeightFluff = overrideHeightFluff;
    InAHallway = inAHallway;
    ForcePlacement = forcePlacement;
    SkipOtherPlatformsCheck = skipOtherPlatformsCheck;
    SkipSpaceCheck = skipSpaceCheck;
    PlaceBooksChance = placeBooksChance;
    NoWaterbolt = noWaterbolt;
    PlacePotsChance = placePotsChance;
    PlaceWaterCandlesChance = placeWaterCandlesChance;
    PlacePotionBottlesChance = placePotionBottlesChance;
    IsAShelf = isAShelf;
  }

  public int PositionX { get; }
  public int PositionY { get; }
  public int? OverrideStyle { get; }
  public int OverrideMaxLengthAllowed { get; }
  public int? OverrideHeightFluff { get; }
  public bool InAHallway { get; }
  public bool ForcePlacement { get; }
  public bool SkipOtherPlatformsCheck { get; }
  public bool SkipSpaceCheck { get; }
  public double PlaceBooksChance { get; }
  public bool NoWaterbolt { get; }
  public double PlacePotsChance { get; }
  public double PlaceWaterCandlesChance { get; }
  public double PlacePotionBottlesChance { get; }
  public bool IsAShelf { get; }
}
