namespace Terraria.Player;

public readonly record struct PlayerItemUseAnimationStepInput(
  int AnimationRemainingTicks,
  int ReuseDelayRemainingTicks,
  bool ControlUseItem,
  bool ReleaseUseItem,
  bool HasPendingReuse);
