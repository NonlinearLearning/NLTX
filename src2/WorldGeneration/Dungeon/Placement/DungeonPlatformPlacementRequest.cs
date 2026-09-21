using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public sealed class DungeonPlatformPlacementRequest
{
  public DungeonPlatformPlacementRequest(DungeonTilePoint position)
  {
    Position = position;
  }

  public DungeonTilePoint Position { get; }

  public int? OverrideStyle { get; init; }

  public int? OverrideMaxLengthAllowed { get; init; }

  public int OverrideHeightFluff { get; init; }

  public bool InAHallway { get; init; }

  public bool ForcePlacement { get; init; }

  public bool SkipOtherPlatformsCheck { get; init; }

  public bool SkipSpaceCheck { get; init; }

  public float PlaceBooksChance { get; init; }

  public bool NoWaterbolt { get; init; }

  public float PlacePotsChance { get; init; }

  public float PlaceWaterCandlesChance { get; init; }

  public float PlacePotionBottlesChance { get; init; }

  public bool IsAShelf =>
    PlaceBooksChance > 0f ||
    PlacePotsChance > 0f ||
    PlaceWaterCandlesChance > 0f ||
    PlacePotionBottlesChance > 0f;

  public Func<DungeonPlacementSnapshot, bool>? CanPlaceHereCallback { get; init; }
}
