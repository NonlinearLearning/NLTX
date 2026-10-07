namespace Terraria.Player;

public readonly record struct PlayerItemUseAnimationStepResult(
  int AnimationRemainingTicks,
  bool HasPendingReuse);
