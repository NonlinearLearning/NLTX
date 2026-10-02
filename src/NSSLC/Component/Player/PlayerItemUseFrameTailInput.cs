namespace Terraria.Player;

public readonly record struct PlayerItemUseFrameTailInput(
  int AnimationRemainingTicks,
  bool ItemIsAir,
  int ItemType,
  bool HasPendingReuse,
  bool ControlUseItem,
  int ItemTimeRemainingTicks);
