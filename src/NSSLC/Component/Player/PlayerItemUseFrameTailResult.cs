namespace Terraria.Player;

public readonly record struct PlayerItemUseFrameTailResult(
  bool ShouldTurnItemToAir,
  bool HasPendingReuse,
  bool ReleaseUseItem,
  int ItemTimeRemainingTicks);
