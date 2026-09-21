using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static PlayerItemSpaceSlotSnapshot[] CreateFullInventory(int typeId = 999)
{
  PlayerItemSpaceSlotSnapshot[] slots = new PlayerItemSpaceSlotSnapshot[58];
  Array.Fill(
    slots,
    new PlayerItemSpaceSlotSnapshot(
      TypeId: typeId,
      PrefixId: 0,
      Stack: 99,
      MaximumStack: 99));
  return slots;
}

PlayerItemSpaceInput personalInventoryInput = new(
  CanTakeItem: true,
  ItemIsGoingToVoidVault: false);
PlayerItemSpaceSnapshot personalInventorySnapshot =
  PlayerItemSpaceQuery.Evaluate(personalInventoryInput);

Require(
  personalInventorySnapshot.CanTakeItem,
  "The item-space query must preserve a positive item acceptance fact.");
Require(
  !personalInventorySnapshot.ItemIsGoingToVoidVault,
  "Personal inventory acceptance must not be marked as VoidVault fallback.");
Require(
  PlayerPersonalInventoryEligibilityQuery.CanTakeItemToPersonalInventory(
    personalInventorySnapshot),
  "An accepted item that is not routed to VoidVault must enter personal inventory.");

PlayerItemSpaceInput voidVaultFallbackInput = new(
  CanTakeItem: true,
  ItemIsGoingToVoidVault: true);
PlayerItemSpaceSnapshot voidVaultFallbackSnapshot =
  PlayerItemSpaceQuery.Evaluate(voidVaultFallbackInput);

Require(
  !PlayerPersonalInventoryEligibilityQuery.CanTakeItemToPersonalInventory(
    voidVaultFallbackSnapshot),
  "VoidVault fallback must not be reported as personal-inventory acceptance.");

PlayerItemSpaceInput rejectedInput = new(
  CanTakeItem: false,
  ItemIsGoingToVoidVault: false);
PlayerItemSpaceSnapshot rejectedSnapshot =
  PlayerItemSpaceQuery.Evaluate(rejectedInput);

Require(
  !PlayerPersonalInventoryEligibilityQuery.CanTakeItemToPersonalInventory(
    rejectedSnapshot),
  "A rejected item must not enter personal inventory.");

Require(
  personalInventorySnapshot == PlayerItemSpaceQuery.Evaluate(personalInventoryInput),
  "The item-space query must be deterministic for the same explicit facts.");

PlayerDashControlSettingsSnapshot defaultDashControl =
  PlayerSettingsAdapter.ReadDashControl(
    new PlayerDashControlSettingsInput(DashControl: null));
Require(
  defaultDashControl.DashControl == PlayerDashControlPreference.AllowDoubleTap,
  "DashControl must use Version4's AllowDoubleTap default when no setting is provided.");

PlayerDashControlSettingsSnapshot hotkeyDashControl =
  PlayerSettingsAdapter.ReadDashControl(
    new PlayerDashControlSettingsInput(
      DashControl: PlayerDashControlPreference.OnlyThroughHotkeys));
Require(
  hotkeyDashControl.DashControl == PlayerDashControlPreference.OnlyThroughHotkeys,
  "DashControl must preserve an explicit OnlyThroughHotkeys setting.");
Require(
  hotkeyDashControl == PlayerSettingsAdapter.ReadDashControl(
    new PlayerDashControlSettingsInput(
      DashControl: PlayerDashControlPreference.OnlyThroughHotkeys)),
  "DashControl adaptation must be deterministic for the same explicit setting.");

PlayerItemSpaceEvaluationInput personalSlotInput = new(
  new PlayerItemSpaceCandidate(
    TypeId: 100,
    PrefixId: 0,
    IsPickup: false,
    IsUniqueStack: false,
    IsCoin: false,
    HasAmmo: false,
    IsNotAmmo: false,
    CanFillEmptyAmmoSlot: false),
  new PlayerItemSpaceSlotSnapshot[]
  {
    new(TypeId: 100, PrefixId: 0, Stack: 1, MaximumStack: 99),
    new(TypeId: 0, PrefixId: 0, Stack: 0, MaximumStack: 0),
  },
  Array.Empty<PlayerItemSpaceSlotSnapshot>(),
  IsVoidVaultEnabled: false,
  CanVoidVaultAccept: false);
PlayerItemSpaceSnapshot personalSlotResult =
  PlayerItemSpaceQuery.Evaluate(personalSlotInput);
Require(
  personalSlotResult.CanTakeItem,
  "ItemSpace must accept a stack-compatible item in the personal inventory scan.");
Require(
  !personalSlotResult.ItemIsGoingToVoidVault,
  "A personal inventory match must not be marked as a VoidVault fallback.");

PlayerItemSpaceSlotSnapshot[] inventoryWithIgnoredTrashSlot = new PlayerItemSpaceSlotSnapshot[59];
inventoryWithIgnoredTrashSlot[58] =
  new PlayerItemSpaceSlotSnapshot(TypeId: 100, PrefixId: 0, Stack: 1, MaximumStack: 99);
PlayerItemSpaceSnapshot ignoredTrashSlotResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 100,
        PrefixId: 0,
        IsPickup: false,
        IsUniqueStack: true,
        IsCoin: false,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      inventoryWithIgnoredTrashSlot,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  ignoredTrashSlotResult.CanTakeItem,
  "Unique-stack detection must preserve Version4's 0-57 inventory range.");

PlayerItemSpaceSlotSnapshot[] uniqueItemInventory = CreateFullInventory();
uniqueItemInventory[57] = new(
  TypeId: 100,
  PrefixId: 0,
  Stack: 1,
  MaximumStack: 99);
PlayerItemSpaceSnapshot uniqueItemResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 100,
        PrefixId: 0,
        IsPickup: false,
        IsUniqueStack: true,
        IsCoin: false,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      uniqueItemInventory,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  !uniqueItemResult.CanTakeItem,
  "An existing unique-stack item in slot 57 must reject the pickup.");

PlayerItemSpaceSlotSnapshot[] coinInventory = CreateFullInventory();
coinInventory[53] = new(
  TypeId: 200,
  PrefixId: 0,
  Stack: 1,
  MaximumStack: 99);
PlayerItemSpaceSnapshot coinResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 200,
        PrefixId: 0,
        IsPickup: false,
        IsUniqueStack: false,
        IsCoin: true,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      coinInventory,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  coinResult.CanTakeItem,
  "Coin pickups must scan the additional personal inventory slots through slot 53.");

PlayerItemSpaceSnapshot nonCoinResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 200,
        PrefixId: 0,
        IsPickup: false,
        IsUniqueStack: false,
        IsCoin: false,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      coinInventory,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  !nonCoinResult.CanTakeItem,
  "Non-coin pickups must not use the coin-only personal inventory slots.");

PlayerItemSpaceSlotSnapshot[] ammoInventory = CreateFullInventory();
ammoInventory[54] = default;
PlayerItemSpaceCandidate ammoCandidate = new(
  TypeId: 300,
  PrefixId: 0,
  IsPickup: false,
  IsUniqueStack: false,
  IsCoin: false,
  HasAmmo: true,
  IsNotAmmo: false,
  CanFillEmptyAmmoSlot: false);
PlayerItemSpaceSnapshot ammoRejectedResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      ammoCandidate,
      ammoInventory,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  !ammoRejectedResult.CanTakeItem,
  "An ammo item that cannot fill an empty ammo slot must be rejected.");

PlayerItemSpaceSnapshot ammoAcceptedResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      ammoCandidate with { CanFillEmptyAmmoSlot = true },
      ammoInventory,
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  ammoAcceptedResult.CanTakeItem,
  "An ammo item that can fill an empty ammo slot must be accepted.");

PlayerItemSpaceSnapshot pickupResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 400,
        PrefixId: 0,
        IsPickup: true,
        IsUniqueStack: true,
        IsCoin: false,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      CreateFullInventory(400),
      Array.Empty<PlayerItemSpaceSlotSnapshot>(),
      IsVoidVaultEnabled: false,
      CanVoidVaultAccept: false));
Require(
  pickupResult.CanTakeItem && !pickupResult.ItemIsGoingToVoidVault,
  "Pickup items must short-circuit ItemSpace before inventory scanning.");

PlayerItemSpaceSnapshot voidVaultResult =
  PlayerItemSpaceQuery.Evaluate(
    new PlayerItemSpaceEvaluationInput(
      new PlayerItemSpaceCandidate(
        TypeId: 500,
        PrefixId: 0,
        IsPickup: false,
        IsUniqueStack: false,
        IsCoin: false,
        HasAmmo: false,
        IsNotAmmo: false,
        CanFillEmptyAmmoSlot: false),
      CreateFullInventory(),
      new[] { default(PlayerItemSpaceSlotSnapshot) },
      IsVoidVaultEnabled: true,
      CanVoidVaultAccept: true));
Require(
  voidVaultResult.CanTakeItem && voidVaultResult.ItemIsGoingToVoidVault,
  "An enabled and accepting VoidVault must be used after inventory capacity is exhausted.");

Console.WriteLine("PASS: item-space eligibility preserves Version4 derived semantics");
