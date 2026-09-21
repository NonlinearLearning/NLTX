namespace Terraria.Player;

public readonly record struct PlayerItemReuseSnapshot(
  int ItemAnimationRemainingTicks,
  int ReuseDelayRemainingTicks,
  bool IsChanneling,
  bool HasPendingReuse);
