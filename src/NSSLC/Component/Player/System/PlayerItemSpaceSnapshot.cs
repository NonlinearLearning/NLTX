namespace Terraria.Player;

public readonly record struct PlayerItemSpaceSnapshot(
  bool CanTakeItem,
  bool ItemIsGoingToVoidVault);
