using Terraria.Player;
using Terraria.Items;
using Terraria.Relationships;
using Terraria.PlayerItemSpaceVerification;

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

static PlayerInventoryItemSnapshot CreateInventoryItem(
  int typeId,
  int stack,
  int maximumStack = 99,
  bool isCoin = false,
  bool hasAmmo = false,
  bool canFillEmptyAmmoSlot = false,
  bool isUniqueStack = false)
{
  ItemEntityRef itemReference = CreateItemReference(Guid.NewGuid());
  return new PlayerInventoryItemSnapshot(
    Entity: itemReference,
    TypeId: typeId,
    PrefixId: 0,
    Stack: stack,
    MaximumStack: maximumStack,
    IsCoin: isCoin,
    HasAmmo: hasAmmo,
    CanFillEmptyAmmoSlot: canFillEmptyAmmoSlot,
    IsUniqueStack: isUniqueStack,
    MutationRevision: CreateMutationRevision(itemReference));
}

static ItemEntityRef CreateItemReference(Guid entityId)
{
  var reference = new EntityReference(
    new EntityUuid(entityId),
    new EntityRuntimeId(Guid.Parse("F0000000-0000-0000-0000-000000000001")),
    EntityReferenceScope.Item);
  return ItemEntityRef.FromReference(reference);
}

static ItemMutationRevision CreateMutationRevision(ItemEntityRef item)
{
  return new ItemMutationRevision(
    item.Reference,
    instanceAttachmentRevision: 1,
    instanceDataRevision: 1,
    stackAttachmentRevision: 1,
    stackDataRevision: 1);
}

static PlayerItemSpaceCandidate CandidateFor(
  PlayerInventoryItemSnapshot item)
{
  return item.ToSpaceCandidate();
}

static void FillInventory(
  PlayerInventorySlotsComponent inventory,
  TestInventoryItemQuery query,
  int typeId = 900)
{
  for (int index = 0; index < inventory.MainInventorySlots.Length; index++)
  {
    PlayerInventoryItemSnapshot item = CreateInventoryItem(typeId, 99);
    inventory.MainInventorySlots[index] = item.Entity;
    query.Items.Add(item.Entity, item);
  }
}

static ItemEntityRef[] CreateEntityRefs(int count)
{
  ItemEntityRef[] entities = new ItemEntityRef[count];
  for (int index = 0; index < count; index++)
  {
    entities[index] = CreateItemReference(Guid.NewGuid());
  }

  return entities;
}

static bool[] CreateHiddenSlots(int count, bool value)
{
  bool[] slots = new bool[count];
  Array.Fill(slots, value);
  return slots;
}

PlayerInventorySlotsComponent emptySlotInventory = new();
TestInventoryItemQuery emptySlotQuery = new();
PlayerInventoryItemSnapshot emptySlotItem = CreateInventoryItem(601, 2);
TestInventoryCommitPort emptySlotPort = new();
PlayerInventoryCommitSystem emptySlotSystem = new(
  emptySlotInventory,
  emptySlotQuery,
  emptySlotPort);
PlayerInventoryCommitResult emptySlotCommit = emptySlotSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    emptySlotItem,
    CandidateFor(emptySlotItem)));
Require(
  emptySlotCommit.Applied &&
  emptySlotCommit.Target.SlotIndex == 49 &&
  emptySlotInventory.MainInventorySlots[49] == emptySlotItem.Entity,
  "Inventory commit must assign an empty main slot using Version4's reverse empty-slot order.");
Require(
  emptySlotPort.LastPlan.HasValue &&
  emptySlotPort.LastPlan.Value.AssignsEmptySlot,
  "An empty-slot commit must expose an atomic assignment plan.");

PlayerInventorySlotsComponent stackInventory = new();
TestInventoryItemQuery stackQuery = new();
PlayerInventoryItemSnapshot occupiedStack = CreateInventoryItem(
  typeId: 602,
  stack: 95);
stackInventory.MainInventorySlots[10] = occupiedStack.Entity;
stackQuery.Items.Add(occupiedStack.Entity, occupiedStack);
TestInventoryCommitPort stackPort = new();
PlayerInventoryCommitSystem stackSystem = new(
  stackInventory,
  stackQuery,
  stackPort);
PlayerInventoryItemSnapshot incomingStack = CreateInventoryItem(
  typeId: 602,
  stack: 10);
PlayerInventoryCommitResult stackCommit = stackSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    incomingStack,
    CandidateFor(incomingStack)));
Require(
  stackCommit.Applied &&
  stackCommit.Target.SlotIndex == 10 &&
  stackCommit.AcceptedStack == 4 &&
  stackCommit.RemainingStack == 6 &&
  stackCommit.IsPartial,
  "Inventory commit must fill a compatible stack and return the remaining item.");
Require(
  stackPort.LastPlan.HasValue &&
  stackPort.LastPlan.Value.ExistingStackAfter == 99,
  "Stack commit must report the target stack after the atomic plan.");

PlayerInventorySlotsComponent uniqueInventory = new();
TestInventoryItemQuery uniqueQuery = new();
PlayerInventoryItemSnapshot existingUnique = CreateInventoryItem(
  typeId: 603,
  stack: 1);
uniqueInventory.MainInventorySlots[0] = existingUnique.Entity;
uniqueQuery.Items.Add(existingUnique.Entity, existingUnique);
TestInventoryCommitPort uniquePort = new();
PlayerInventoryCommitSystem uniqueSystem = new(
  uniqueInventory,
  uniqueQuery,
  uniquePort);
PlayerInventoryItemSnapshot duplicateUnique = CreateInventoryItem(
  typeId: 603,
  stack: 1,
  isUniqueStack: true);
PlayerInventoryCommitResult uniqueCommit = uniqueSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    duplicateUnique,
    CandidateFor(duplicateUnique)));
Require(
  !uniqueCommit.Applied &&
  uniqueCommit.RejectionReason ==
  PlayerInventoryCommitRejectionReason.UniqueItemAlreadyPresent &&
  uniquePort.ApplyCount == 0,
  "Unique items must reject before the commit port can write state.");

PlayerInventorySlotsComponent ammoCommitInventory = new();
TestInventoryItemQuery ammoQuery = new();
FillInventory(ammoCommitInventory, ammoQuery);
PlayerInventoryItemSnapshot emptyAmmoSlotItem =
  ammoQuery.Items[ammoCommitInventory.MainInventorySlots[54]];
ammoQuery.Items.Remove(emptyAmmoSlotItem.Entity);
ammoCommitInventory.MainInventorySlots[54] = ItemEntityRef.None;
TestInventoryCommitPort ammoPort = new();
PlayerInventoryCommitSystem ammoSystem = new(
  ammoCommitInventory,
  ammoQuery,
  ammoPort);
PlayerInventoryItemSnapshot ammoItem = CreateInventoryItem(
  typeId: 604,
  stack: 5,
  hasAmmo: true,
  canFillEmptyAmmoSlot: true);
PlayerInventoryCommitResult ammoCommit = ammoSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    ammoItem,
    CandidateFor(ammoItem)));
Require(
  ammoCommit.Applied &&
  ammoCommit.Target.SlotIndex == 54,
  "Ammo commits must use the first eligible slot in Version4's 54-57 range.");

PlayerInventorySlotsComponent coinCommitInventory = new();
TestInventoryItemQuery coinQuery = new();
FillInventory(coinCommitInventory, coinQuery);
PlayerInventoryItemSnapshot coinStack = CreateInventoryItem(
  typeId: 71,
  stack: 1,
  isCoin: true);
coinCommitInventory.MainInventorySlots[50] = coinStack.Entity;
coinQuery.Items[coinStack.Entity] = coinStack;
PlayerInventoryItemSnapshot replacedCoin = coinQuery.Items[
  coinCommitInventory.MainInventorySlots[51]];
coinQuery.Items.Remove(replacedCoin.Entity);
coinCommitInventory.MainInventorySlots[51] = ItemEntityRef.None;
TestInventoryCommitPort coinPort = new();
PlayerInventoryCommitSystem coinSystem = new(
  coinCommitInventory,
  coinQuery,
  coinPort);
PlayerInventoryItemSnapshot incomingCoin = CreateInventoryItem(
  typeId: 71,
  stack: 2,
  isCoin: true);
PlayerInventoryCommitResult coinCommit = coinSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    incomingCoin,
    CandidateFor(incomingCoin)));
Require(
  coinCommit.Applied &&
  coinCommit.Target.SlotIndex == 50,
  "Coin commits must try the additional personal slots before ordinary slots.");

PlayerInventorySlotsComponent voidInventory = new();
TestInventoryItemQuery voidQuery = new()
{
  IsVoidVaultEnabled = true,
  VoidVaultAccept = true,
};
FillInventory(voidInventory, voidQuery);
voidQuery.VoidVaultItems.Add(default);
TestInventoryCommitPort voidPort = new();
PlayerInventoryCommitSystem voidSystem = new(
  voidInventory,
  voidQuery,
  voidPort);
PlayerInventoryItemSnapshot voidItem = CreateInventoryItem(605, 1);
PlayerInventoryCommitResult voidCommit = voidSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    voidItem,
    CandidateFor(voidItem)));
Require(
  voidCommit.Applied &&
  voidCommit.Target.IsVoidVault &&
  voidCommit.Target.SlotIndex == 0,
  "VoidVault must be selected only after personal inventory capacity is exhausted.");

PlayerInventorySlotsComponent rejectedInventory = new();
TestInventoryItemQuery rejectedQuery = new();
TestInventoryCommitPort rejectedPort = new()
{
  ShouldAccept = false,
};
PlayerInventoryCommitSystem rejectedSystem = new(
  rejectedInventory,
  rejectedQuery,
  rejectedPort);
PlayerInventoryItemSnapshot rejectedItem = CreateInventoryItem(606, 1);
PlayerInventoryCommitResult rejectedCommit = rejectedSystem.Commit(
  new PlayerInventoryCommitCommand(
    Guid.NewGuid(),
    rejectedItem,
    CandidateFor(rejectedItem)));
Require(
  !rejectedCommit.Applied &&
  rejectedCommit.RejectionReason ==
  PlayerInventoryCommitRejectionReason.CommitPortRejected &&
  rejectedInventory.MainInventorySlots.All(item => item.IsEmpty),
  "A rejected commit port must not cause a partial player-slot write.");

Guid duplicateCommandId = Guid.NewGuid();
PlayerInventorySlotsComponent duplicateInventory = new();
TestInventoryItemQuery duplicateQuery = new();
TestInventoryCommitPort duplicatePort = new();
PlayerInventoryCommitSystem duplicateSystem = new(
  duplicateInventory,
  duplicateQuery,
  duplicatePort);
PlayerInventoryItemSnapshot duplicateItem = CreateInventoryItem(607, 1);
PlayerInventoryCommitCommand duplicateCommand = new(
  duplicateCommandId,
  duplicateItem,
  CandidateFor(duplicateItem));
Require(
  duplicateSystem.Commit(duplicateCommand).Applied,
  "The first inventory command must commit.");
PlayerInventoryCommitResult duplicateCommit =
  duplicateSystem.Commit(duplicateCommand);
Require(
  !duplicateCommit.Applied &&
  duplicateCommit.RejectionReason ==
  PlayerInventoryCommitRejectionReason.DuplicateCommand,
  "A successfully committed command must be idempotently rejected on replay.");

PlayerInventorySlotsComponent pickupInventory = new();
TestInventoryItemQuery pickupQuery = new();
TestInventoryCommitPort pickupCommitPort = new();
PlayerInventoryCommitSystem pickupCommitSystem = new(
  pickupInventory,
  pickupQuery,
  pickupCommitPort);
PlayerCoinMergeSystem pickupCoinMergeSystem = new(
  pickupInventory,
  pickupQuery,
  new TestInventoryCoinMergePort(pickupInventory, pickupQuery));
RecordingInventoryEffectPort pickupEffectPort = new();
PlayerInventoryPickupSystem pickupSystem = new(
  pickupCommitSystem,
  pickupCoinMergeSystem,
  pickupEffectPort);
PlayerInventoryItemSnapshot pickupItem = CreateInventoryItem(608, 2);
PlayerInventoryPickupResult pickupCompositionResult = pickupSystem.Process(
  new PlayerInventoryPickupCommand(
    Guid.NewGuid(),
    pickupItem,
    CandidateFor(pickupItem),
    PlayerInventoryTransferSettings.PickupItemFromWorld));
Require(
  pickupCompositionResult.Applied &&
  pickupCompositionResult.EffectsApplied &&
  pickupCompositionResult.RemainingStack == 0 &&
  pickupEffectPort.Intents.Select(intent => intent.Kind).SequenceEqual(
    new[]
    {
      PlayerInventoryEffectIntentKind.PickupSound,
      PlayerInventoryEffectIntentKind.PickupLog,
      PlayerInventoryEffectIntentKind.PickupText,
      PlayerInventoryEffectIntentKind.Achievement,
      PlayerInventoryEffectIntentKind.PostAction,
    }),
  "Inventory pickup composition must preserve the effect-intent order of GetItem.");

PlayerInventorySlotsComponent restrictedPickupInventory = new();
TestInventoryItemQuery restrictedPickupQuery = new();
FillInventory(restrictedPickupInventory, restrictedPickupQuery, typeId: 609);
restrictedPickupQuery.VoidVaultItems.Add(default);
PlayerInventoryCommitSystem restrictedCommitSystem = new(
  restrictedPickupInventory,
  restrictedPickupQuery,
  new TestInventoryCommitPort());
PlayerInventoryPickupSystem restrictedPickupSystem = new(
  restrictedCommitSystem,
  new PlayerCoinMergeSystem(
    restrictedPickupInventory,
    restrictedPickupQuery,
    new TestInventoryCoinMergePort(
      restrictedPickupInventory,
      restrictedPickupQuery)),
  new RecordingInventoryEffectPort());
PlayerInventoryItemSnapshot restrictedItem = CreateInventoryItem(610, 1);
PlayerInventoryPickupResult restrictedResult = restrictedPickupSystem.Process(
  new PlayerInventoryPickupCommand(
    Guid.NewGuid(),
    restrictedItem,
    CandidateFor(restrictedItem),
    new PlayerInventoryTransferSettings(CanGoIntoVoidVault: false)));
Require(
  !restrictedResult.Applied &&
  restrictedResult.Inventory.RejectionReason ==
  PlayerInventoryCommitRejectionReason.NoSpace,
  "Inventory transfer settings must prevent an implicit VoidVault fallback.");

PlayerInventorySlotsComponent quietPickupInventory = new();
TestInventoryItemQuery quietPickupQuery = new();
RecordingInventoryEffectPort quietEffectPort = new();
PlayerInventoryItemSnapshot quietItem = CreateInventoryItem(611, 1);
PlayerInventoryPickupSystem quietPickupSystem = new(
  new PlayerInventoryCommitSystem(
    quietPickupInventory,
    quietPickupQuery,
    new TestInventoryCommitPort()),
  new PlayerCoinMergeSystem(
    quietPickupInventory,
    quietPickupQuery,
    new TestInventoryCoinMergePort(
      quietPickupInventory,
      quietPickupQuery)),
  quietEffectPort);
PlayerInventoryPickupResult quietResult = quietPickupSystem.Process(
  new PlayerInventoryPickupCommand(
    Guid.NewGuid(),
    quietItem,
    CandidateFor(quietItem),
    new PlayerInventoryTransferSettings(
      NoSound: true,
      NoText: true)));
Require(
  quietResult.Applied &&
  quietEffectPort.Intents.Select(intent => intent.Kind).SequenceEqual(
    new[]
    {
      PlayerInventoryEffectIntentKind.PickupLog,
      PlayerInventoryEffectIntentKind.Achievement,
      PlayerInventoryEffectIntentKind.PostAction,
    }),
  "Inventory transfer settings must suppress only the requested sound and text intents.");

PlayerInventorySlotsComponent coinMergeInventory = new();
TestInventoryItemQuery coinMergeQuery = new();
PlayerInventoryItemSnapshot copperCoins = CreateInventoryItem(
  typeId: 71,
  stack: 100,
  maximumStack: 999,
  isCoin: true);
PlayerInventoryItemSnapshot silverCoins = CreateInventoryItem(
  typeId: 72,
  stack: 99,
  maximumStack: 999,
  isCoin: true);
coinMergeInventory.MainInventorySlots[0] = copperCoins.Entity;
coinMergeInventory.MainInventorySlots[3] = silverCoins.Entity;
coinMergeQuery.Items.Add(copperCoins.Entity, copperCoins);
coinMergeQuery.Items.Add(silverCoins.Entity, silverCoins);
TestInventoryCoinMergePort coinMergePort = new(
  coinMergeInventory,
  coinMergeQuery);
PlayerCoinMergeSystem coinMergeSystem = new(
  coinMergeInventory,
  coinMergeQuery,
  coinMergePort);
PlayerCoinMergeResult coinMergeResult = coinMergeSystem.Merge(
  new PlayerCoinMergeCommand(Guid.NewGuid(), SourceSlotIndex: 0));
PlayerInventoryItemSnapshot mergedCoins =
  coinMergeQuery.Items[silverCoins.Entity];
Require(
  coinMergeResult.Applied &&
  coinMergeResult.AppliedStepCount == 2 &&
  coinMergeResult.TerminalSlotIndex == 3 &&
  coinMergeInventory.MainInventorySlots[0].IsEmpty &&
  mergedCoins.TypeId == 73 &&
  mergedCoins.Stack == 1,
  "Coin merge must preserve Version4's upgrade, destination increment, and recursive merge order.");

PlayerEquipmentRelationComponent loadoutEquipment = new();
PlayerAppearanceSelectionComponent loadoutAppearance = new();
PlayerLoadoutStateComponent loadoutState = new();

ItemEntityRef[] playerArmorBefore = CreateEntityRefs(
  PlayerEquipmentRelationComponent.ArmorSlotCount);
ItemEntityRef[] playerDyesBefore = CreateEntityRefs(
  PlayerEquipmentRelationComponent.DyeSlotCount);
bool[] playerHiddenBefore = CreateHiddenSlots(
  PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
  true);
Array.Copy(playerArmorBefore, loadoutEquipment.ArmorSlots,
  playerArmorBefore.Length);
Array.Copy(playerDyesBefore, loadoutEquipment.DyeSlots,
  playerDyesBefore.Length);
Array.Copy(playerHiddenBefore, loadoutAppearance.HiddenVisibleAccessories,
  playerHiddenBefore.Length);

ItemEntityRef[] currentLoadoutArmor = CreateEntityRefs(
  PlayerEquipmentRelationComponent.ArmorSlotCount);
ItemEntityRef[] currentLoadoutDyes = CreateEntityRefs(
  PlayerEquipmentRelationComponent.DyeSlotCount);
bool[] currentLoadoutHidden = CreateHiddenSlots(
  PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
  false);
ItemEntityRef[] targetLoadoutArmor = CreateEntityRefs(
  PlayerEquipmentRelationComponent.ArmorSlotCount);
ItemEntityRef[] targetLoadoutDyes = CreateEntityRefs(
  PlayerEquipmentRelationComponent.DyeSlotCount);
bool[] targetLoadoutHidden = CreateHiddenSlots(
  PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
  true);

ItemEntityRef[] loadout0Armor =
  (ItemEntityRef[])loadoutState.Loadouts[0].Equipment;
ItemEntityRef[] loadout0Dyes =
  (ItemEntityRef[])loadoutState.Loadouts[0].Dyes;
bool[] loadout0Hidden =
  (bool[])loadoutState.Loadouts[0].HiddenAccessories;
ItemEntityRef[] loadout1Armor =
  (ItemEntityRef[])loadoutState.Loadouts[1].Equipment;
ItemEntityRef[] loadout1Dyes =
  (ItemEntityRef[])loadoutState.Loadouts[1].Dyes;
bool[] loadout1Hidden =
  (bool[])loadoutState.Loadouts[1].HiddenAccessories;
Array.Copy(currentLoadoutArmor, loadout0Armor, currentLoadoutArmor.Length);
Array.Copy(currentLoadoutDyes, loadout0Dyes, currentLoadoutDyes.Length);
Array.Copy(currentLoadoutHidden, loadout0Hidden, currentLoadoutHidden.Length);
Array.Copy(targetLoadoutArmor, loadout1Armor, targetLoadoutArmor.Length);
Array.Copy(targetLoadoutDyes, loadout1Dyes, targetLoadoutDyes.Length);
Array.Copy(targetLoadoutHidden, loadout1Hidden, targetLoadoutHidden.Length);

PlayerLoadoutSystem loadoutSystem = new(
  loadoutEquipment,
  loadoutAppearance,
  loadoutState);
Guid loadoutCommandId = Guid.NewGuid();
PlayerLoadoutSwitchResult loadoutResult = loadoutSystem.Switch(
  new PlayerLoadoutSwitchCommand(
    loadoutCommandId,
    TargetLoadoutIndex: 1,
    PlayerIndex: 1,
    MainPlayerIndex: 0,
    UsingOrReusingItem: false,
    CCed: false,
    Dead: false));
Require(
  loadoutResult.Applied &&
  loadoutResult.PreviousLoadoutIndex == 0 &&
  loadoutResult.CurrentLoadoutIndex == 1 &&
  loadoutState.CurrentLoadoutIndex == 1,
  "Loadout switching must commit the target index after the swap.");
Require(
  loadoutEquipment.ArmorSlots.SequenceEqual(targetLoadoutArmor) &&
  loadoutEquipment.DyeSlots.SequenceEqual(targetLoadoutDyes) &&
  loadoutAppearance.HiddenVisibleAccessories.SequenceEqual(targetLoadoutHidden),
  "Loadout switching must expose the target armor, dye, and hidden-accessory state.");
Require(
  ((ItemEntityRef[])loadoutState.Loadouts[0].Equipment)
    .SequenceEqual(playerArmorBefore) &&
  ((ItemEntityRef[])loadoutState.Loadouts[0].Dyes)
    .SequenceEqual(playerDyesBefore) &&
  ((bool[])loadoutState.Loadouts[0].HiddenAccessories)
    .SequenceEqual(playerHiddenBefore) &&
  ((ItemEntityRef[])loadoutState.Loadouts[1].Equipment)
    .SequenceEqual(currentLoadoutArmor) &&
  ((ItemEntityRef[])loadoutState.Loadouts[1].Dyes)
    .SequenceEqual(currentLoadoutDyes) &&
  ((bool[])loadoutState.Loadouts[1].HiddenAccessories)
    .SequenceEqual(currentLoadoutHidden),
  "Loadout switching must preserve Version4's two-swap exchange order.");

PlayerLoadoutSwitchResult duplicateLoadoutResult = loadoutSystem.Switch(
  new PlayerLoadoutSwitchCommand(
    loadoutCommandId,
    TargetLoadoutIndex: 0,
    PlayerIndex: 1,
    MainPlayerIndex: 0,
    UsingOrReusingItem: false,
    CCed: false,
    Dead: false));
Require(
  !duplicateLoadoutResult.Applied &&
  duplicateLoadoutResult.RejectionReason ==
  PlayerLoadoutSwitchRejectionReason.DuplicateCommand &&
  loadoutState.CurrentLoadoutIndex == 1,
  "A committed loadout command must reject replay without changing state.");

PlayerLoadoutSwitchResult blockedLoadoutResult = loadoutSystem.Switch(
  new PlayerLoadoutSwitchCommand(
    Guid.NewGuid(),
    TargetLoadoutIndex: 0,
    PlayerIndex: 0,
    MainPlayerIndex: 0,
    UsingOrReusingItem: true,
    CCed: false,
    Dead: false));
Require(
  !blockedLoadoutResult.Applied &&
  blockedLoadoutResult.RejectionReason ==
  PlayerLoadoutSwitchRejectionReason.SwitchingBlocked &&
  loadoutState.CurrentLoadoutIndex == 1 &&
  loadoutEquipment.ArmorSlots.SequenceEqual(targetLoadoutArmor),
  "A local loadout switch while using an item must reject before mutation.");
Require(
  loadoutEquipment.Revision == 1,
  "A committed loadout switch must advance the shared equipment relation revision.");

PlayerAccessoryVisibilitySystem visibilitySystem =
  new(loadoutAppearance);
PlayerLoadoutNetworkSystem loadoutNetworkSystem =
  new(loadoutSystem, visibilitySystem);
ushort acceptedVisibilityMask = 0x155;
Guid networkCommandId = Guid.NewGuid();
PlayerLoadoutNetworkResult networkLoadoutResult =
  loadoutNetworkSystem.Process(
    new PlayerLoadoutNetworkRequest(
      networkCommandId,
      PacketPlayerIndex: 7,
      AuthorityPlayerIndex: 1,
      MainPlayerIndex: 0,
      TargetLoadoutIndex: 0,
      UsingOrReusingItem: false,
      CCed: false,
      Dead: false,
      VisibilityMask: acceptedVisibilityMask));
Require(
  networkLoadoutResult.Loadout.Applied &&
  networkLoadoutResult.Visibility.Applied &&
  networkLoadoutResult.VisibilityAttemptedAfterLoadout &&
  networkLoadoutResult.PacketPlayerIndex == 7 &&
  networkLoadoutResult.AuthorityPlayerIndex == 1 &&
  networkLoadoutResult.Visibility.Snapshot.VisibilityMask ==
    acceptedVisibilityMask,
  "Packet loadout composition must switch first and then apply the ten-bit visibility mask.");
Require(
  loadoutAppearance.HiddenVisibleAccessories.SequenceEqual(
    new[] { true, false, true, false, true, false, true, false, true, false }),
  "Accessory visibility must expand mask bits in low-to-high slot order.");

ushort rejectedSwitchVisibilityMask = 0x2AA;
PlayerLoadoutNetworkResult rejectedSwitchNetworkResult =
  loadoutNetworkSystem.Process(
    new PlayerLoadoutNetworkRequest(
      Guid.NewGuid(),
      PacketPlayerIndex: 7,
      AuthorityPlayerIndex: 1,
      MainPlayerIndex: 0,
      TargetLoadoutIndex: 0,
      UsingOrReusingItem: false,
      CCed: false,
      Dead: false,
      VisibilityMask: rejectedSwitchVisibilityMask));
Require(
  !rejectedSwitchNetworkResult.Loadout.Applied &&
  rejectedSwitchNetworkResult.Loadout.RejectionReason ==
    PlayerLoadoutSwitchRejectionReason.SameLoadout &&
  rejectedSwitchNetworkResult.Visibility.Applied &&
  rejectedSwitchNetworkResult.Visibility.Snapshot.VisibilityMask ==
    rejectedSwitchVisibilityMask,
  "A rejected loadout switch must still expose the packet-order visibility result.");

PlayerLoadoutNetworkResult duplicateNetworkResult =
  loadoutNetworkSystem.Process(
    new PlayerLoadoutNetworkRequest(
      networkCommandId,
      PacketPlayerIndex: 7,
      AuthorityPlayerIndex: 1,
      MainPlayerIndex: 0,
      TargetLoadoutIndex: 0,
      UsingOrReusingItem: false,
      CCed: false,
      Dead: false,
      VisibilityMask: acceptedVisibilityMask));
Require(
  !duplicateNetworkResult.Loadout.Applied &&
  duplicateNetworkResult.Loadout.RejectionReason ==
    PlayerLoadoutSwitchRejectionReason.DuplicateCommand &&
  !duplicateNetworkResult.Visibility.Applied &&
  duplicateNetworkResult.Visibility.RejectionReason ==
    PlayerAccessoryVisibilityRejectionReason.DuplicateCommand &&
  visibilitySystem.Snapshot().VisibilityMask == rejectedSwitchVisibilityMask,
  "A replayed packet must not mutate loadout or visibility state twice.");

using MemoryStream packet147Stream =
  new(new byte[] { 7, 1, 0x55, 0x01 });
using BinaryReader packet147Reader = new(packet147Stream);
PlayerLoadoutPacket147DecodeResult decodedPacket147 =
  PlayerLoadoutPacket147Adapter.Decode(
    packet147Reader,
    Guid.NewGuid(),
    authorityPlayerIndex: 1,
    mainPlayerIndex: 0,
    usingOrReusingItem: false,
    cced: false,
    dead: false);
Require(
  decodedPacket147.Decoded &&
  decodedPacket147.Request is { PacketPlayerIndex: 7,
    AuthorityPlayerIndex: 1,
    TargetLoadoutIndex: 1,
    VisibilityMask: 0x155 },
  "Packet-147 decoding must preserve the wire player byte and resolve authority separately.");

using MemoryStream truncatedPacket147Stream =
  new(new byte[] { 7, 1, 0x55 });
using BinaryReader truncatedPacket147Reader =
  new(truncatedPacket147Stream);
PlayerLoadoutPacket147DecodeResult truncatedPacket147 =
  PlayerLoadoutPacket147Adapter.Decode(
    truncatedPacket147Reader,
    Guid.NewGuid(),
    authorityPlayerIndex: 1,
    mainPlayerIndex: 0,
    usingOrReusingItem: false,
    cced: false,
    dead: false);
Require(
  !truncatedPacket147.Decoded &&
  truncatedPacket147.Status == PlayerLoadoutPacket147DecodeStatus.Truncated,
  "Packet-147 decoding must reject a truncated visibility mask without a partial request.");

using MemoryStream stagedTruncatedPacket147Stream =
  new(new byte[] { 7, 1 });
using BinaryReader stagedTruncatedPacket147Reader =
  new(stagedTruncatedPacket147Stream);
PlayerLoadoutPacket147ProcessResult stagedTruncatedPacket147 =
  loadoutNetworkSystem.ProcessPacket147(
    stagedTruncatedPacket147Reader,
    Guid.NewGuid(),
    authorityPlayerIndex: 1,
    mainPlayerIndex: 0,
    usingOrReusingItem: false,
    cced: false,
    dead: false);
Require(
  stagedTruncatedPacket147.Status ==
    PlayerLoadoutPacket147ProcessStatus.VisibilityMaskTruncated &&
  stagedTruncatedPacket147.Loadout.Applied &&
  stagedTruncatedPacket147.VisibilityReadAfterLoadout &&
  loadoutState.CurrentLoadoutIndex == 1 &&
  visibilitySystem.Snapshot().VisibilityMask == 0x03FF,
  $"Staged packet processing must preserve Version4's switch-before-mask read order on truncation. status={stagedTruncatedPacket147.Status}, loadoutApplied={stagedTruncatedPacket147.Loadout.Applied}, loadoutReason={stagedTruncatedPacket147.Loadout.RejectionReason}, current={loadoutState.CurrentLoadoutIndex}, visibilityReadAfterLoadout={stagedTruncatedPacket147.VisibilityReadAfterLoadout}, visibilityMask=0x{visibilitySystem.Snapshot().VisibilityMask:X4}");

using MemoryStream headerTruncatedPacket147Stream =
  new(new byte[] { 7 });
using BinaryReader headerTruncatedPacket147Reader =
  new(headerTruncatedPacket147Stream);
PlayerLoadoutPacket147ProcessResult headerTruncatedPacket147 =
  loadoutNetworkSystem.ProcessPacket147(
    headerTruncatedPacket147Reader,
    Guid.NewGuid(),
    authorityPlayerIndex: 1,
    mainPlayerIndex: 0,
    usingOrReusingItem: false,
    cced: false,
    dead: false);
Require(
  headerTruncatedPacket147.Status ==
    PlayerLoadoutPacket147ProcessStatus.HeaderTruncated &&
  !headerTruncatedPacket147.VisibilityReadAfterLoadout &&
  loadoutState.CurrentLoadoutIndex == 1 &&
  visibilitySystem.Snapshot().VisibilityMask == 0x03FF,
  "Staged packet processing must reject a truncated header before switching loadout or applying visibility.");

using MemoryStream appliedPacket147Stream =
  new(new byte[] { 7, 0, 0x01, 0x00 });
using BinaryReader appliedPacket147Reader = new(appliedPacket147Stream);
PlayerLoadoutPacket147ProcessResult appliedPacket147 =
  loadoutNetworkSystem.ProcessPacket147(
    appliedPacket147Reader,
    Guid.NewGuid(),
    authorityPlayerIndex: 1,
    mainPlayerIndex: 0,
    usingOrReusingItem: false,
    cced: false,
    dead: false);
Require(
  appliedPacket147.Status == PlayerLoadoutPacket147ProcessStatus.Applied &&
  appliedPacket147.Loadout.Applied &&
  appliedPacket147.Visibility.Applied &&
  appliedPacket147.VisibilityReadAfterLoadout &&
  loadoutState.CurrentLoadoutIndex == 0 &&
  visibilitySystem.Snapshot().VisibilityMask == 0x0001,
  "A complete staged packet must switch loadout before applying its visibility mask.");

PlayerEquipmentRelationComponent equipmentRelations = new();
PlayerEquipmentCommitSystem equipmentSystem =
  new(equipmentRelations);
ItemEntityRef helmet = CreateItemReference(Guid.NewGuid());
PlayerEquipmentCommitResult armorCommit = equipmentSystem.Commit(
  new PlayerEquipmentCommitCommand(
    Guid.NewGuid(),
    PlayerEquipmentSlotKind.Armor,
    SlotIndex: 0,
    helmet,
    ExpectedCurrentItem: ItemEntityRef.None,
    ExpectedRevision: 0));
Require(
  armorCommit.Applied &&
  equipmentRelations.ArmorSlots[0] == helmet &&
  armorCommit.Revision == 1 &&
  armorCommit.EffectRebuildRequired,
  "Equipment commit must atomically write an armor relation and expose effect invalidation.");

Guid duplicateEquipmentCommandId = Guid.NewGuid();
PlayerEquipmentCommitResult duplicateEquipmentResult = equipmentSystem.Commit(
  new PlayerEquipmentCommitCommand(
    duplicateEquipmentCommandId,
    PlayerEquipmentSlotKind.Dye,
    SlotIndex: 0,
    helmet,
    ExpectedCurrentItem: ItemEntityRef.None,
    ExpectedRevision: 1));
Require(
  !duplicateEquipmentResult.Applied &&
  duplicateEquipmentResult.RejectionReason ==
  PlayerEquipmentCommitRejectionReason.ItemAlreadyEquipped &&
  equipmentRelations.DyeSlots[0].IsEmpty &&
  equipmentRelations.Revision == 1,
  "An item already related to another equipment slot must reject without a partial write.");

PlayerEquipmentCommitResult staleEquipmentResult = equipmentSystem.Commit(
  new PlayerEquipmentCommitCommand(
    Guid.NewGuid(),
    PlayerEquipmentSlotKind.MiscEquipment,
    SlotIndex: 4,
    CreateItemReference(Guid.NewGuid()),
    ExpectedRevision: 0));
Require(
  !staleEquipmentResult.Applied &&
  staleEquipmentResult.RejectionReason ==
  PlayerEquipmentCommitRejectionReason.StaleExpectedRevision &&
  equipmentRelations.MiscEquipmentSlots[4].IsEmpty,
  "A stale equipment revision must reject before changing a relation.");

ItemEntityRef replacement = CreateItemReference(Guid.NewGuid());
PlayerEquipmentCommitResult replacementResult = equipmentSystem.Commit(
  new PlayerEquipmentCommitCommand(
    Guid.NewGuid(),
    PlayerEquipmentSlotKind.Armor,
    SlotIndex: 0,
    replacement,
    ExpectedCurrentItem: helmet,
    ExpectedRevision: 1));
Require(
  replacementResult.Applied &&
  replacementResult.PreviousItem == helmet &&
  replacementResult.CurrentItem == replacement &&
  equipmentRelations.ArmorSlots[0] == replacement &&
  equipmentRelations.Revision == 2,
  "Equipment replacement must honor the expected current relation and advance revision.");

Guid replayEquipmentCommandId = Guid.NewGuid();
PlayerEquipmentCommitCommand unequipCommand = new(
  replayEquipmentCommandId,
  PlayerEquipmentSlotKind.Armor,
  SlotIndex: 0,
  ItemEntityRef.None,
  ExpectedCurrentItem: replacement,
  ExpectedRevision: 2);
PlayerEquipmentCommitResult unequipResult = equipmentSystem.Commit(unequipCommand);
PlayerEquipmentCommitResult replayUnequipResult = equipmentSystem.Commit(unequipCommand);
Require(
  unequipResult.Applied &&
  equipmentRelations.ArmorSlots[0].IsEmpty &&
  replayUnequipResult.RejectionReason ==
  PlayerEquipmentCommitRejectionReason.DuplicateCommand &&
  equipmentRelations.Revision == 3,
  "Unequip must clear the relation once and reject replay without another mutation.");

PlayerEquipmentCommitResult invalidEquipmentIndexResult = equipmentSystem.Commit(
  new PlayerEquipmentCommitCommand(
    Guid.NewGuid(),
    PlayerEquipmentSlotKind.MiscDye,
    SlotIndex: PlayerEquipmentRelationComponent.MiscDyeSlotCount,
    CreateItemReference(Guid.NewGuid())));
Require(
  !invalidEquipmentIndexResult.Applied &&
  invalidEquipmentIndexResult.RejectionReason ==
  PlayerEquipmentCommitRejectionReason.InvalidSlotIndex &&
  equipmentRelations.Revision == 3,
  "Equipment commit must reject an out-of-range misc-dye slot without changing revision.");

TestEquipmentVisualItemQuery visualQuery = new();
ItemEntityRef baseArmor = CreateItemReference(Guid.NewGuid());
ItemEntityRef vanityArmor = CreateItemReference(Guid.NewGuid());
ItemEntityRef headDye = CreateItemReference(Guid.NewGuid());
ItemEntityRef bodyDye = CreateItemReference(Guid.NewGuid());
ItemEntityRef legDye = CreateItemReference(Guid.NewGuid());
visualQuery.Items[baseArmor] = new PlayerEquipmentVisualItemSnapshot(
  baseArmor,
  TypeId: 100,
  DyeId: 0,
  HeadSlot: 4,
  HandOnSlot: 2,
  BackSlot: 3,
  ShieldSlot: 1,
  FaceSlot: 5);
visualQuery.Items[vanityArmor] = new PlayerEquipmentVisualItemSnapshot(
  vanityArmor,
  TypeId: 101,
  DyeId: 0,
  HeadSlot: 9);
visualQuery.Items[headDye] = new PlayerEquipmentVisualItemSnapshot(
  headDye,
  TypeId: 200,
  DyeId: 7);
visualQuery.Items[bodyDye] = new PlayerEquipmentVisualItemSnapshot(
  bodyDye,
  TypeId: 201,
  DyeId: 8);
visualQuery.Items[legDye] = new PlayerEquipmentVisualItemSnapshot(
  legDye,
  TypeId: 202,
  DyeId: 9);
PlayerEquipmentRelationComponent projectionEquipment = new();
projectionEquipment.ArmorSlots[0] = baseArmor;
projectionEquipment.ArmorSlots[10] = vanityArmor;
projectionEquipment.DyeSlots[0] = headDye;
projectionEquipment.DyeSlots[1] = bodyDye;
projectionEquipment.DyeSlots[2] = legDye;
PlayerAppearanceSelectionComponent projectionAppearance = new();
PlayerEquipmentColorProjectionComponent projectionColors = new();
PlayerVisibleEquipmentSelectionComponent visibleSelection = new();
PlayerEquipmentProjectionInput projectionInput = new(
  Enumerable.Repeat(true, PlayerEquipmentRelationComponent.ArmorSlotCount).ToArray(),
  Enumerable.Repeat(false, PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount)
    .ToArray(),
  WearsRobe: false);
PlayerEquipmentProjectionResult projectionResult =
  new PlayerEquipmentProjectionSystem().Project(
    projectionEquipment,
    projectionAppearance,
    visualQuery,
    projectionColors,
    visibleSelection,
    projectionInput);
Require(
  projectionResult.Applied &&
  projectionResult.MissingItemMetadataCount == 0 &&
  projectionColors.CHead == 7 &&
  projectionColors.CBody == 8 &&
  projectionColors.CLegs == 9 &&
  projectionColors.CHandOn == 7 &&
  projectionColors.CShield == 7 &&
  projectionColors.CShieldFallback == 7 &&
  visibleSelection.Head == 9,
  "Equipment projection must preserve dye order, shield fallback, and vanity head override.");

PlayerEquipmentProjectionResult invalidProjectionResult =
  new PlayerEquipmentProjectionSystem().Project(
    projectionEquipment,
    projectionAppearance,
    visualQuery,
    projectionColors,
    visibleSelection,
    projectionInput with { UsableArmorSlots = Array.Empty<bool>() });
Require(
  !invalidProjectionResult.Applied &&
  invalidProjectionResult.RejectionReason ==
  PlayerEquipmentProjectionRejectionReason.InvalidUsableArmorSlotCount &&
  projectionColors.CHead == 7,
  "Equipment projection must reject malformed slot inputs before mutating derived output.");

PlayerEquipmentEffectStateComponent effectState = new();
PlayerEquipmentEffectSystem effectSystem = new();
PlayerEquipmentEffectSnapshot effectSnapshot = effectSystem.Rebuild(
  effectState,
  new PlayerEquipmentEffectRebuildInput(
    ArmorEffectDrawShadow: true,
    ArmorEffectDrawShadowSubtle: false,
    ArmorEffectDrawOutlines: true,
    ArmorEffectDrawShadowLokis: false,
    ArmorEffectDrawShadowBasilisk: true,
    ArmorEffectDrawOutlinesForbidden: false,
    ArmorEffectDrawShadowEocShield: true,
    SocialShadowRocketBoots: false,
    SocialGhost: true,
    AshWoodBonus: true,
    SocialIgnoreLight: false));
Require(
  effectSnapshot.ArmorEffectDrawShadow &&
  effectSnapshot.ArmorEffectDrawOutlines &&
  effectSnapshot.ArmorEffectDrawShadowBasilisk &&
  effectSnapshot.SocialGhost &&
  effectSnapshot.AshWoodBonus,
  "Equipment effect rebuild must copy the explicit effect capability facts.");
PlayerEquipmentEffectSnapshot resetEffectSnapshot = effectSystem.ResetForTick(effectState);
Require(
  resetEffectSnapshot == new PlayerEquipmentEffectSnapshot(
    false, false, false, false, false, false, false, false, false, false, false),
  "Equipment effect reset must clear all rebuildable P09 flags before the next tick.");

PlayerDefenseStateComponent defenseState = new();
PlayerInteractionLockStateComponent interactionLockState = new();
PlayerDefenseInteractionSystem defenseSystem = new();
Guid raiseShieldCommandId = Guid.NewGuid();
PlayerDefenseInteractionResult raiseShieldResult = defenseSystem.ToggleShield(
  defenseState,
  new PlayerShieldToggleCommand(raiseShieldCommandId, ShouldGuard: true));
Require(
  raiseShieldResult.Applied &&
  raiseShieldResult.ResetItemTimers &&
  defenseState.ShieldRaised &&
  defenseState.ShieldParryTimeLeft == 1,
  "Shield raise must open the parry window and request item-timer reset.");
defenseSystem.AdvanceTick(defenseState);
Require(
  defenseState.ShieldParryTimeLeft == 2,
  "Shield parry time must advance once per tick while the window is active.");
PlayerDefenseInteractionResult lowerShieldResult = defenseSystem.ToggleShield(
  defenseState,
  new PlayerShieldToggleCommand(Guid.NewGuid(), ShouldGuard: false));
Require(
  lowerShieldResult.Applied &&
  !defenseState.ShieldRaised &&
  defenseState.ShieldParryTimeLeft == 0 &&
  defenseState.ShieldParryCooldown == 15 &&
  lowerShieldResult.AttackCooldownFrames == 20,
  "Shield release must start the parry cooldown and expose the Combat attack cooldown request.");
PlayerDefenseInteractionResult duplicateShieldResult = defenseSystem.ToggleShield(
  defenseState,
  new PlayerShieldToggleCommand(raiseShieldCommandId, ShouldGuard: true));
Require(
  duplicateShieldResult.RejectionReason ==
  PlayerDefenseInteractionRejectionReason.DuplicateCommand &&
  !defenseState.ShieldRaised,
  "A replayed shield command must not toggle defense state twice.");
defenseSystem.LockTileInteractions(interactionLockState);
defenseSystem.UpdateTileInteractions(
  interactionLockState,
  tileInteractAttempted: false,
  mouseInterface: false);
defenseSystem.UpdateTileInteractions(
  interactionLockState,
  tileInteractAttempted: false,
  mouseInterface: false);
defenseSystem.UpdateTileInteractions(
  interactionLockState,
  tileInteractAttempted: false,
  mouseInterface: false);
PlayerTileInteractionUpdateResult releasedTileInteraction =
  defenseSystem.UpdateTileInteractions(
    interactionLockState,
    tileInteractAttempted: false,
    mouseInterface: false);
Require(
  releasedTileInteraction.ReleaseUseTile &&
  releasedTileInteraction.LockTileInteractionsTimer == 0,
  "Tile interaction lock must last three ticks before release becomes available.");

PlayerStatusEffectSystem coordinatorStatusSystem = new(
  new PlayerBuffSlotsComponent(),
  new PlayerBuffImmunityComponent(8),
  new PlayerStatusEffectCatalog(
    [new PlayerStatusEffectDefinition(
      new ContentId<BuffDefinition>(3),
      isDebuff: false)]));
PlayerEquipmentEffectStateComponent coordinatorEffectState = new();
PlayerEquipmentRelationComponent coordinatorEquipment = new();
PlayerAppearanceSelectionComponent coordinatorAppearance = new();
PlayerEquipmentColorProjectionComponent coordinatorColors = new();
PlayerVisibleEquipmentSelectionComponent coordinatorVisible = new();
PlayerDefenseStateComponent coordinatorDefense = new();
PlayerTickCoordinatorResult coordinatorResult = new PlayerTickCoordinator().Tick(
  coordinatorEffectState,
  coordinatorEquipment,
  coordinatorAppearance,
  visualQuery,
  coordinatorColors,
  coordinatorVisible,
  coordinatorStatusSystem,
  coordinatorDefense,
  projectionInput,
  new PlayerEquipmentEffectRebuildInput(
    ArmorEffectDrawShadow: true,
    ArmorEffectDrawShadowSubtle: false,
    ArmorEffectDrawOutlines: false,
    ArmorEffectDrawShadowLokis: false,
    ArmorEffectDrawShadowBasilisk: false,
    ArmorEffectDrawOutlinesForbidden: false,
    ArmorEffectDrawShadowEocShield: false,
    SocialShadowRocketBoots: false,
    SocialGhost: false,
    AshWoodBonus: false,
    SocialIgnoreLight: false),
  new PlayerStatusEffectTickInput(DecrementTimers: false));
Require(
  coordinatorResult.Projection.Applied &&
  coordinatorResult.Effects.ArmorEffectDrawShadow &&
  coordinatorResult.Buffs.ActiveCount == 0 &&
  coordinatorResult.Defense.ShieldParryCooldown == 0,
  "Player tick coordination must preserve the explicit reset, projection, buff, effect, and defense phases.");

PlayerResourceStateComponent coordinatedResourceState = new();
PlayerBuffResourceSystem coordinatedResourceSystem =
  new(coordinatedResourceState);
PlayerBuffResourceRebuildInput coordinatedResourceInput =
  new(
    BreathMax: 200,
    Breath: 180,
    LavaMax: 10,
    LavaTime: 4,
    IgnoreWater: true,
    LavaVision: true,
    LavaOpacity: 0.8f);
PlayerTickCoordinatorResult resourceCoordinatorResult =
  new PlayerTickCoordinator().Tick(
    coordinatorEffectState,
    coordinatorEquipment,
    coordinatorAppearance,
    visualQuery,
    coordinatorColors,
    coordinatorVisible,
    coordinatorStatusSystem,
    coordinatorDefense,
    projectionInput,
    new PlayerEquipmentEffectRebuildInput(
      ArmorEffectDrawShadow: false,
      ArmorEffectDrawShadowSubtle: false,
      ArmorEffectDrawOutlines: false,
      ArmorEffectDrawShadowLokis: false,
      ArmorEffectDrawShadowBasilisk: false,
      ArmorEffectDrawOutlinesForbidden: false,
      ArmorEffectDrawShadowEocShield: false,
      SocialShadowRocketBoots: false,
      SocialGhost: false,
      AshWoodBonus: false,
      SocialIgnoreLight: false),
    new PlayerStatusEffectTickInput(DecrementTimers: false),
    coordinatedResourceSystem,
    coordinatedResourceInput,
    new PlayerBuffResourceTickInput(LavaWet: false));
Require(
  resourceCoordinatorResult.ResourceRebuild is { Applied: true } &&
  resourceCoordinatorResult.Resources is
    { Breath: 180, LavaMax: 10, LavaTime: 5, IgnoreWater: true, LavaVision: true } &&
  Math.Abs(resourceCoordinatorResult.Resources.Value.LavaOpacity - 0.84f) < 0.0001f,
  "Player tick coordination must reset, rebuild, and advance resource state in the Version4 order.");

PlayerResourceStateComponent resourceState = new();
PlayerBuffResourceSystem resourceSystem = new(resourceState);
PlayerBuffResourceRebuildResult resourceRebuild = resourceSystem.Rebuild(
  new PlayerBuffResourceRebuildInput(
    BreathMax: 200,
    Breath: 100,
    LavaMax: 420,
    LavaTime: 0,
    IgnoreWater: true,
    LavaVision: true,
    LavaOpacity: 1f));
Require(
  resourceRebuild.Applied &&
  resourceRebuild.Snapshot.Breath == 100 &&
  resourceRebuild.Snapshot.LavaMax == 420 &&
  resourceRebuild.Snapshot.IgnoreWater &&
  resourceRebuild.Snapshot.LavaVision,
  "Buff resource rebuild must commit validated breath, lava, and capability facts.");
PlayerBuffResourceRebuildResult invalidResourceRebuild = resourceSystem.Rebuild(
  new PlayerBuffResourceRebuildInput(
    BreathMax: 200,
    Breath: 100,
    LavaMax: 420,
    LavaTime: 0,
    IgnoreWater: false,
    LavaVision: false,
    LavaOpacity: float.NaN));
Require(
  !invalidResourceRebuild.Applied &&
  invalidResourceRebuild.RejectionReason ==
  PlayerBuffResourceRebuildRejectionReason.InvalidLavaOpacity &&
  resourceSystem.Snapshot().LavaVision,
  "Invalid lava opacity must reject without replacing the committed resource facts.");
PlayerBuffResourceSnapshot dryResourceTick = resourceSystem.AdvanceTick(
  new PlayerBuffResourceTickInput(LavaWet: false));
Require(
  dryResourceTick.LavaTime == 1 &&
  dryResourceTick.LavaOpacity == 1f,
  "Lava time must recover while dry and opacity must stay capped at one.");
PlayerBuffResourceSnapshot wetResourceTick = resourceSystem.AdvanceTick(
  new PlayerBuffResourceTickInput(LavaWet: true));
Require(
  wetResourceTick.LavaTime == 1 &&
  wetResourceTick.LavaOpacity == 0.96f,
  "Lava vision must reduce opacity by the Version4 step while wet.");
PlayerBuffResourceSnapshot resetResource = resourceSystem.ResetForTick();
Require(
  resetResource.LavaMax == 0 &&
  !resetResource.IgnoreWater &&
  !resetResource.LavaVision &&
  resetResource.LavaTime == 0,
  "Buff resource tick reset must clear derived lava capabilities without touching breath.");

PlayerBuffSlotsComponent buffSlots = new();
PlayerBuffImmunityComponent buffImmunity = new(8);
PlayerStatusEffectSystem buffSystem = new(
  buffSlots,
  buffImmunity,
  new PlayerStatusEffectCatalog(
    [new PlayerStatusEffectDefinition(
      new ContentId<BuffDefinition>(1),
      isDebuff: false)]));
ContentId<BuffDefinition> buffType = new(1);
buffImmunity.SetImmunity(buffType, isImmune: true);
buffSystem.ResetForTick();
PlayerStatusEffectResult buffApplied = buffSystem.Apply(
  new PlayerStatusEffectApplyCommand(buffType, DurationTicks: 4));
Require(
  buffApplied.Applied &&
  !buffImmunity.IsImmune(buffType) &&
  buffSystem.Snapshot().ActiveCount == 1,
  "Buff reset must clear immunity before the status commit phase.");
buffSystem.ResetForLifecycle();
Require(
  buffSystem.Snapshot().ActiveCount == 0 &&
  !buffImmunity.IsImmune(buffType),
  "Buff lifecycle reset must clear paired slots and immunity state.");

PlayerContainerRelationComponent containerRelations = new();
PlayerContainerRelationSystem containerSystem = new(containerRelations);
PlayerContainerRef bankRef = new(Guid.NewGuid());
PlayerContainerRelationResult bankBindResult = containerSystem.Apply(
  new PlayerContainerRelationCommand(
    Guid.NewGuid(),
    PlayerContainerRelationSlot.Bank,
    bankRef,
    IsAvailable: true,
    IsOpen: false));
Require(
  bankBindResult.Applied &&
  bankBindResult.CurrentContainer == bankRef &&
  containerRelations.Bank == bankRef,
  "A bank relation command must bind the player relation without copying container contents.");

PlayerContainerRelationResult bankUnbindResult = containerSystem.Apply(
  new PlayerContainerRelationCommand(
    Guid.NewGuid(),
    PlayerContainerRelationSlot.Bank,
    PlayerContainerRef.None,
    IsAvailable: false,
    IsOpen: false));
Require(
  bankUnbindResult.Applied &&
  containerRelations.Bank.IsEmpty,
  "A bank relation command must support an explicit unbind.");

PlayerContainerRelationResult voidVaultRelationResult = containerSystem.Apply(
  new PlayerContainerRelationCommand(
    Guid.NewGuid(),
    PlayerContainerRelationSlot.VoidVault,
    PlayerContainerRef.None,
    IsAvailable: true,
    IsOpen: true));
Require(
  voidVaultRelationResult.Applied &&
  containerRelations.VoidVaultState == new VoidVaultState(true, true),
  "VoidVault relation state must preserve availability and open capability.");

PlayerContainerRelationResult invalidVoidVaultRelationResult = containerSystem.Apply(
  new PlayerContainerRelationCommand(
    Guid.NewGuid(),
    PlayerContainerRelationSlot.VoidVault,
    PlayerContainerRef.None,
    IsAvailable: false,
    IsOpen: true));
Require(
  !invalidVoidVaultRelationResult.Applied &&
  invalidVoidVaultRelationResult.RejectionReason ==
  PlayerContainerRelationRejectionReason.InvalidVoidVaultState &&
  containerRelations.VoidVaultState == new VoidVaultState(true, true),
  "An open but unavailable VoidVault must reject without changing relation state.");

Console.WriteLine(
  "PASS: player inventory, equipment, defense, loadout-switch, and container-relation core semantics");

RuntimeItemOwnershipVerification.Run();

sealed class TestInventoryItemQuery : IPlayerInventoryItemQuery
{
  public Dictionary<ItemEntityRef, PlayerInventoryItemSnapshot> Items { get; } = [];

  public List<PlayerInventoryItemSnapshot> VoidVaultItems { get; } = [];

  IReadOnlyList<PlayerInventoryItemSnapshot>
    IPlayerInventoryItemQuery.VoidVaultItems => VoidVaultItems;

  public bool IsVoidVaultEnabled { get; init; }

  public bool VoidVaultAccept { get; init; }

  public bool TryGetItem(
    ItemEntityRef item,
    out PlayerInventoryItemSnapshot snapshot)
  {
    return Items.TryGetValue(item, out snapshot);
  }

  public bool CanVoidVaultAccept(PlayerInventoryItemSnapshot item)
  {
    return VoidVaultAccept;
  }
}

sealed class TestInventoryCommitPort : IPlayerInventoryCommitPort
{
  public bool ShouldAccept { get; init; } = true;

  public int ApplyCount { get; private set; }

  public PlayerInventoryCommitPlan? LastPlan { get; private set; }

  public bool TryApply(in PlayerInventoryCommitPlan plan)
  {
    ApplyCount++;
    LastPlan = plan;
    return ShouldAccept;
  }
}

sealed class TestInventoryCoinMergePort : IPlayerInventoryCoinMergePort
{
  private readonly PlayerInventorySlotsComponent _inventory;
  private readonly TestInventoryItemQuery _query;

  public TestInventoryCoinMergePort(
    PlayerInventorySlotsComponent inventory,
    TestInventoryItemQuery query)
  {
    _inventory = inventory;
    _query = query;
  }

  public bool TryApply(in PlayerCoinMergePlan plan)
  {
    if (!plan.HasDestination)
    {
      PlayerInventoryItemSnapshot source = _query.Items[plan.SourceItem];
      _query.Items[plan.SourceItem] = source with
      {
        TypeId = plan.UpgradedSourceTypeId,
        PrefixId = 0,
        Stack = 1,
      };
      return true;
    }

    _inventory.MainInventorySlots[plan.SourceSlotIndex] = ItemEntityRef.None;
    PlayerInventoryItemSnapshot destination =
      _query.Items[plan.DestinationItem];
    _query.Items[plan.DestinationItem] = destination with
    {
      Stack = plan.DestinationStackAfter,
    };
    return true;
  }
}

sealed class RecordingInventoryEffectPort : IPlayerInventoryEffectPort
{
  public List<PlayerInventoryEffectIntent> Intents { get; } = [];

  public bool TryApply(in PlayerInventoryEffectIntent intent)
  {
    Intents.Add(intent);
    return true;
  }
}

sealed class TestEquipmentVisualItemQuery : IPlayerEquipmentVisualItemQuery
{
  public Dictionary<ItemEntityRef, PlayerEquipmentVisualItemSnapshot> Items { get; } = [];

  public bool TryGetItem(
    ItemEntityRef item,
    out PlayerEquipmentVisualItemSnapshot snapshot)
  {
    return Items.TryGetValue(item, out snapshot);
  }
}
