namespace Terraria.Player;

public readonly record struct PlayerItemUseChannelContinuationInput(
  bool ControlUseItem,
  bool ControlUseTile,
  bool MountTypeEightActive,
  bool SelectedItemIsKite,
  bool HasBufferedSelectionChange);
