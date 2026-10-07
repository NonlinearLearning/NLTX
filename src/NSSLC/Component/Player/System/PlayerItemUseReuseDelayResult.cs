namespace Terraria.Player;

public readonly record struct PlayerItemUseReuseDelayResult(
  int AnimationRemainingTicks,
  int ItemTimeRemainingTicks,
  int ReuseDelayRemainingTicks);
