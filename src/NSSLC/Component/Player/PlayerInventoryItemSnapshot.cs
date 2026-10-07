using Terraria.Items;

namespace Terraria.Player;

public readonly record struct PlayerInventoryItemSnapshot(
  ItemEntityRef Entity,
  int TypeId,
  int PrefixId,
  int Stack,
  int MaximumStack,
  bool IsFavorited = false,
  bool IsOnlyNeedOneInInventory = false,
  bool IsUniqueStack = false,
  bool IsCoin = false,
  bool HasAmmo = false,
  bool IsNotAmmo = false,
  bool CanFillEmptyAmmoSlot = false,
  ItemMutationRevision MutationRevision = default)
{
  public bool IsEmpty =>
    Entity.IsEmpty ||
    TypeId <= 0 ||
    Stack <= 0;

  public PlayerItemSpaceCandidate ToSpaceCandidate(bool isPickup = false)
  {
    return new PlayerItemSpaceCandidate(
      TypeId,
      PrefixId,
      isPickup,
      IsUniqueStack,
      IsCoin,
      HasAmmo,
      IsNotAmmo,
      CanFillEmptyAmmoSlot,
      IsFavorited);
  }

  public PlayerItemSpaceSlotSnapshot ToSlotSnapshot()
  {
    if (IsEmpty)
    {
      return default;
    }

    return new PlayerItemSpaceSlotSnapshot(
      TypeId,
      PrefixId,
      Stack,
      MaximumStack,
      IsFavorited,
      IsOnlyNeedOneInInventory);
  }
}
