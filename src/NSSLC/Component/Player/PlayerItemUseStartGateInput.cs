namespace Terraria.Player;

public readonly record struct PlayerItemUseStartGateInput(
  bool ControlUseItem,
  bool ReleaseUseItem,
  int AnimationRemainingTicks,
  int ItemUseStyle,
  bool HasBufferedSelectionChange);
