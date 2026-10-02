namespace Terraria.Player;

public readonly record struct PlayerItemSpaceCandidate(
  int TypeId,
  int PrefixId,
  bool IsPickup,
  bool IsUniqueStack,
  bool IsCoin,
  bool HasAmmo,
  bool IsNotAmmo,
  bool CanFillEmptyAmmoSlot,
  bool IsFavorited = false,
  bool HasUseStyle = false);
