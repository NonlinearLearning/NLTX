namespace Terraria.Player;

public readonly record struct PlayerTileInteractionUpdateResult(
  bool ReleaseUseTile,
  int LockTileInteractionsTimer);
