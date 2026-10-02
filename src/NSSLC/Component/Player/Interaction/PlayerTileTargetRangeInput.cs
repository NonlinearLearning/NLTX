namespace Terraria.Player.Interaction;

public readonly record struct PlayerTileTargetRangeInput(
  bool IsLocalPlayer,
  bool IsDisplayDollOrInanimate,
  bool IsJourneyMode,
  bool FarPlacementRangePowerUnlocked,
  bool FarPlacementRangePowerEnabledForPlayer,
  int CurrentTileRangeX,
  int CurrentTileRangeY);
