namespace Terraria.WorldGeneration.Housing;

/// <summary>
/// Tile facts already classified by the world Tile owner for Version4 room scoring.
/// </summary>
public readonly record struct HousingRoomScoreTileSample(
  bool IsActive,
  bool IsSolid,
  bool IsIgnoredInHouseScore,
  bool IsBasicChest,
  bool IsDoorTile,
  bool IsOpenDoorAnchorFrame,
  int GoodEvilBalanceContribution,
  bool IsHomeSpotForbidden,
  int ScoreContribution)
{
  public bool IsSolidActive => IsActive && IsSolid;
}
