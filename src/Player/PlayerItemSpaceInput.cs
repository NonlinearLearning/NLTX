namespace Terraria.Player;

public readonly record struct PlayerItemSpaceInput(
  bool CanTakeItem,
  bool ItemIsGoingToVoidVault);
