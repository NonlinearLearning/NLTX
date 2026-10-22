namespace MinimalImplementation.Server.Inventory.Content;

/// <summary>
/// Immutable SetDefaults/ItemID facts. IsAmmo is ammo > 0 and !notAmmo for ItemSpace;
/// null leaves that fact unavailable. FitsAmmoSlot also covers bait and paint for GetItem.
/// </summary>
public sealed record ItemPickupFacts(
  bool FitsAmmoSlot,
  bool CanFillEmptyAmmoSlot,
  bool HasUseStyle,
  bool UniqueStack,
  bool OnlyNeedOneInInventory,
  bool QuestItem,
  bool? IsAmmo = null);
