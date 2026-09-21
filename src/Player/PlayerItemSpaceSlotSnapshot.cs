namespace Terraria.Player;

public readonly record struct PlayerItemSpaceSlotSnapshot(
  int TypeId,
  int PrefixId,
  int Stack,
  int MaximumStack,
  bool IsFavorited = false,
  bool OnlyNeedOneInInventory = false);
