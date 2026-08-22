using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Compatibility;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Items.Snapshots;
using Terraria.Dome.Simulation.Items.Systems;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Inventory.Systems;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

ItemDefinitionRegistry definitions = new([new ItemDefinition(1, 99)]);
InventoryComponent inventory = new();
InventoryTransferSystem transfers = new();
int remainder = transfers.TransferIntoSlot(inventory, 0, new ItemStack(1, 70), definitions);
if (remainder != 0 || inventory.GetSlot(0) != new ItemStack(1, 70))
{
  throw new InvalidOperationException("Inventory did not accept an authoritative initial stack.");
}

remainder = transfers.TransferIntoSlot(inventory, 0, new ItemStack(1, 50), definitions);
if (remainder != 21 || inventory.GetSlot(0) != new ItemStack(1, 99))
{
  throw new InvalidOperationException("Inventory did not enforce authoritative stack limits.");
}

long inventoryRevision = inventory.Revision;
inventory.SetSlot(1, new ItemStack(1, 2));
if (inventory.Revision <= inventoryRevision)
{
  throw new InvalidOperationException("Inventory mutations did not advance its revision.");
}

inventory.SetSelectedSlot(0);
if (inventory.SelectedSlot != 0)
{
  throw new InvalidOperationException("Inventory did not preserve a valid selected slot.");
}

try
{
  inventory.SetSelectedSlot(InventoryComponent.SlotCount);
  throw new InvalidOperationException("Inventory accepted an invalid selected slot.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = transfers.TransferIntoSlot(inventory, 0, new ItemStack(2, 1), definitions);
  throw new InvalidOperationException("Inventory accepted an item missing from the server registry.");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("PASS: inventory slots, selected slot and stack limits are authoritative");

ItemDefinitionRegistry metadataDefinitions = new([
  new ItemDefinition(1, 99),
  new ItemDefinition(
    2,
    1,
    Identity: new ItemIdentityDefinition("Item.Unique", UniqueStack: true)),
  new ItemDefinition(3, 999),
  new ItemDefinition(
    4,
    1,
    Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 5)),
  new ItemDefinition(
    5,
    99,
    Placement: new ItemPlacementDefinition(TileType: 12))]);
InventoryComponent metadataInventory = new();
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent prefixOne =
  new(PrefixId: 1);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent prefixTwo =
  new(PrefixId: 2);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent variantOne =
  new(VariantId: 1);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent dyed =
  new(Dye: 4);
int metadataRemainder = transfers.TransferIntoSlot(
  metadataInventory,
  0,
  new ItemStack(1, 20),
  prefixOne,
  metadataDefinitions);
if (metadataRemainder != 0 || metadataInventory.GetInstanceState(0) != prefixOne)
{
  throw new InvalidOperationException("Item instance state was not retained with its inventory slot.");
}

metadataRemainder = transfers.TransferIntoSlot(
  metadataInventory,
  0,
  new ItemStack(1, 1),
  prefixTwo,
  metadataDefinitions);
if (metadataRemainder != 1)
{
  throw new InvalidOperationException("Different prefixes were merged into one item stack.");
}

metadataRemainder = transfers.TransferIntoSlot(
  metadataInventory,
  0,
  new ItemStack(1, 1),
  variantOne,
  metadataDefinitions);
if (metadataRemainder != 1)
{
  throw new InvalidOperationException("Different variants were merged into one item stack.");
}

metadataRemainder = transfers.TransferIntoSlot(
  metadataInventory,
  0,
  new ItemStack(1, 1),
  dyed,
  metadataDefinitions);
if (metadataRemainder != 1)
{
  throw new InvalidOperationException("Different dyes were merged into one item stack.");
}

metadataInventory.SetSlot(1, new ItemStack(2, 1));
metadataRemainder = transfers.TransferIntoSlot(
  metadataInventory,
  1,
  new ItemStack(2, 1),
  metadataDefinitions);
if (metadataRemainder != 1)
{
  throw new InvalidOperationException("Unique-stack items were merged.");
}

metadataInventory.SetSlot(15, new ItemStack(1, 2));
if (metadataInventory.GetSlot(15) != new ItemStack(1, 2))
{
  throw new InvalidOperationException("Inventory did not retain a non-hotbar item slot.");
}

try
{
  metadataInventory.SetSelectedSlot(InventoryComponent.HotbarSlotCount);
  throw new InvalidOperationException("A non-hotbar item slot was selected as an active item.");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("PASS: item instance metadata and full inventory slots preserve merge invariants");

WorldItemSpawnSystem worldItemSpawn = new();
WorldItemMotionSystem worldItemMotion = new();
WorldItemPickupDelaySystem worldItemPickupDelay = new();
WorldItemDestroySystem worldItemDestroy = new();
WorldItemPickupSystem worldItemPickup = new();
WorldItemStackingSystem worldItemStacking = new();
int nextWorldItemId = 1;
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent worldItemInstanceState =
  new(PrefixId: 3, IsFavorited: true);
if (!worldItemSpawn.TryCreate(
    ref nextWorldItemId,
    new CreateWorldItemCommand(
      new ItemStack(1, 5),
      new SimulationVector(0.0f, 0.0f),
      default,
      SpawnSource: 1,
      InstanceState: worldItemInstanceState),
    out WorldItemComponent worldItem,
    out _))
{
    throw new InvalidOperationException("World item ECS spawn rejected a valid stack.");
}

if (worldItem.InstanceState != worldItemInstanceState)
{
  throw new InvalidOperationException("World item ECS spawn discarded item instance state.");
}

if (worldItemSpawn.TryCreate(
    ref nextWorldItemId,
    new CreateWorldItemCommand(
      new ItemStack(1, 1),
      new SimulationVector(float.NaN, 0.0f),
      default,
      SpawnSource: 1),
    out _,
    out _))
{
  throw new InvalidOperationException("World item ECS spawn accepted a non-finite position.");
}

if (!worldItemSpawn.TryCreate(
    ref nextWorldItemId,
    new CreateWorldItemCommand(
      new ItemStack(1, 1),
      new SimulationVector(0.0f, 0.0f),
      default,
      SpawnSource: 1,
      PickupDelayTicks: 2),
    out WorldItemComponent delayedWorldItem,
    out _) ||
    delayedWorldItem.WorldState.PickupDelayTicks != 2 ||
    !worldItemPickupDelay.TryAdvance(delayedWorldItem, out delayedWorldItem) ||
    delayedWorldItem.WorldState.PickupDelayTicks != 1 ||
    delayedWorldItem.Revision != 2 ||
    delayedWorldItem.WorldState.Revision != 2)
{
  throw new InvalidOperationException(
    "World item pickup delay did not spawn and advance as revisioned state.");
}

WorldItemComponent movedWorldItem;
if (!worldItemMotion.TryMove(
    worldItem,
    new MoveWorldItemCommand(worldItem.ReplicationId, new SimulationVector(1.0f, 0.0f), 1),
    default,
    out movedWorldItem) || movedWorldItem.Revision != 2)
{
  throw new InvalidOperationException("World item motion did not advance its revision deterministically.");
}

if (worldItemMotion.TryMove(
    movedWorldItem,
    new MoveWorldItemCommand(
      movedWorldItem.ReplicationId,
      new SimulationVector(float.NaN, 0.0f),
      movedWorldItem.Revision),
    default,
    out _))
{
  throw new InvalidOperationException("World item motion accepted a non-finite position.");
}

if (!worldItemDestroy.TryDestroy(
    movedWorldItem,
    new DestroyWorldItemCommand(movedWorldItem.ReplicationId, movedWorldItem.Revision),
    out WorldItemComponent destroyedWorldItem,
    out WorldItemDestroyedEvent destroyedWorldItemEvent) ||
    destroyedWorldItem.IsActive || !destroyedWorldItem.Stack.IsEmpty ||
    destroyedWorldItem.Revision != 3 ||
    destroyedWorldItemEvent != new WorldItemDestroyedEvent(worldItem.ReplicationId, 3))
{
  throw new InvalidOperationException(
    "World item destruction did not produce a revisioned inactive tombstone.");
}

InventoryComponent pickupInventory = new();
WorldItemComponent snapshotWorldItem = movedWorldItem;
if (!worldItemPickup.TryPickup(
    ref movedWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(1), movedWorldItem.ReplicationId),
    new SimulationVector(1.0f, 0.0f),
    pickupInventory,
    definitions,
    pickupRange: 3.0f,
    out _,
    out _)
    || movedWorldItem.IsActive
    || pickupInventory.GetSlot(0) != new ItemStack(1, 5)
    || pickupInventory.GetInstanceState(0) != worldItemInstanceState)
{
  throw new InvalidOperationException("World item ECS pickup did not commit exactly once.");
}

if (worldItemPickup.TryPickup(
    ref movedWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(2), movedWorldItem.ReplicationId),
    new SimulationVector(1.0f, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: 3.0f,
    out _,
    out _))
{
  throw new InvalidOperationException("A second player won an inactive world item pickup.");
}

WorldItemComponent reservedWorldItem = new(
  98,
  new ItemStack(1, 1),
  new SimulationVector(1.0f, 0.0f),
  true,
  1,
  default,
  new Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent(
    IsActive: true,
    PickupDelayTicks: 0,
    SpawnSource: 1,
    LastOwnerRevision: 0,
    LastMergeTick: -1,
    Revision: 1,
    ReservedPlayerId: 1,
    ReservationAgeTicks: 0));
if (reservedWorldItem.WorldState.CanBePickedUpBy(new PlayerHandle(2)) ||
    !reservedWorldItem.WorldState.CanBePickedUpBy(new PlayerHandle(1)))
{
  throw new InvalidOperationException(
    "World item reservation did not authorize only its reserved player.");
}

Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent invalidReservation =
  reservedWorldItem.WorldState with
{
  ReservedPlayerId = 256
};
if (invalidReservation.CanBePickedUpBy(new PlayerHandle(1)))
{
  throw new InvalidOperationException("An invalid world item reservation id was accepted.");
}

if (reservedWorldItem.WorldState.CanBePickedUpBy(default))
{
  throw new InvalidOperationException("World item reservation accepted an invalid player handle.");
}

if (worldItemPickup.TryPickup(
    ref reservedWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(2), reservedWorldItem.ReplicationId),
    new SimulationVector(1.0f, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: 3.0f,
    out _,
    out _))
{
  throw new InvalidOperationException(
    "A non-reserved player bypassed the world item owner reservation.");
}

if (!worldItemPickup.TryPickup(
    ref reservedWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(1), reservedWorldItem.ReplicationId),
    new SimulationVector(1.0f, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: 3.0f,
    out _,
    out _))
{
  throw new InvalidOperationException(
    "The reserved player could not pick up its reserved world item.");
}

Console.WriteLine("PASS: world item owner reservations gate pickup authorization");

WorldItemComponent rangeWorldItem = new(
  99,
  new ItemStack(1, 1),
  new SimulationVector(100.0f, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(1));
if (worldItemPickup.TryPickup(
    ref rangeWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(1), 99),
    new SimulationVector(0.0f, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: float.NaN,
    out _,
    out _))
{
  throw new InvalidOperationException("World item pickup accepted a non-finite pickup range.");
}

if (worldItemPickup.TryPickup(
    ref rangeWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(1), 99),
    new SimulationVector(float.NaN, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: 3.0f,
    out _,
    out _))
{
  throw new InvalidOperationException("World item pickup accepted a non-finite player position.");
}

WorldItemComponent invalidPositionWorldItem = rangeWorldItem with
{
  Position = new SimulationVector(float.NaN, 0.0f)
};
if (worldItemPickup.TryPickup(
    ref invalidPositionWorldItem,
    new PickupWorldItemCommand(new PlayerHandle(1), 99),
    new SimulationVector(0.0f, 0.0f),
    new InventoryComponent(),
    definitions,
    pickupRange: 3.0f,
    out _,
    out _))
{
  throw new InvalidOperationException("World item pickup accepted a non-finite item position.");
}

if (!worldItemStacking.TryMerge(
    new WorldItemComponent(
      1,
      new ItemStack(1, 95),
      new SimulationVector(0.0f, 0.0f),
      true,
      1,
      default,
      Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(1)),
    new WorldItemComponent(
      2,
      new ItemStack(1, 10),
      new SimulationVector(0.5f, 0.0f),
      true,
      1,
      default,
      Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(2)),
    definitions,
    tick: 8,
    maximumDistance: 1.0f,
    out WorldItemComponent mergedReceiver,
    out WorldItemComponent mergedDonor) ||
    mergedReceiver.Stack != new ItemStack(1, 99) ||
    mergedDonor.Stack != new ItemStack(1, 6) ||
    mergedReceiver.WorldState.LastMergeTick != 8 ||
    mergedDonor.WorldState.LastMergeTick != 8)
{
  throw new InvalidOperationException(
    "Compatible nearby world items did not passively merge with deterministic revisions.");
}

WorldItemComponent finiteReceiver = new(
  1,
  new ItemStack(1, 1),
  new SimulationVector(float.NaN, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(1));
WorldItemComponent finiteDonor = new(
  2,
  new ItemStack(1, 1),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(2));
if (worldItemStacking.TryMerge(
      finiteReceiver,
      finiteDonor,
      definitions,
      tick: 8,
      maximumDistance: 1.0f,
      out _,
      out _) ||
    worldItemStacking.TryMerge(
      finiteDonor,
      new WorldItemComponent(
        3,
        new ItemStack(1, 1),
        new SimulationVector(0.0f, 0.0f),
        true,
        1,
        default,
        Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(3)),
      definitions,
      tick: 8,
      maximumDistance: float.NaN,
      out _,
      out _))
{
  throw new InvalidOperationException("World item stacking accepted non-finite geometry input.");
}

Console.WriteLine("PASS: compatible world items passively merge under stack limits");

using (DomeSimulation worldStackingSimulation = new(new WorldGrid(400, 300)))
{
  int receiverId = worldStackingSimulation.SpawnWorldItem(
    new ItemStack(1, 95),
    new SimulationVector(20.0f, 20.0f));
  int donorId = worldStackingSimulation.SpawnWorldItem(
    new ItemStack(1, 10),
    new SimulationVector(20.5f, 20.0f));
  worldStackingSimulation.Tick(new SimulationInputBatch());
  ItemReplicationSnapshot receiverSnapshot = worldStackingSimulation
    .CreateItemReplicationSnapshots()
    .Single(item => item.ReplicationId == receiverId);
  ItemReplicationSnapshot donorSnapshot = worldStackingSimulation
    .CreateItemReplicationSnapshots()
    .Single(item => item.ReplicationId == donorId);
  if (receiverSnapshot.Stack != new ItemStack(1, 99) ||
      donorSnapshot.Stack != new ItemStack(1, 6) ||
      receiverSnapshot.WorldState.LastMergeTick != worldStackingSimulation.TickNumber ||
      donorSnapshot.WorldState.LastMergeTick != worldStackingSimulation.TickNumber)
  {
    throw new InvalidOperationException(
      "Simulation did not commit passive world-item stacking through the ECS store.");
  }
}

Console.WriteLine("PASS: Simulation commits passive world-item stacking");

Console.WriteLine("PASS: ECS world item spawn, revisioned motion and one-winner pickup");

ItemInputValidationSystem inputValidation = new();
ItemInputValidationResult validInput = inputValidation.Validate(
  true,
  metadataInventory,
  0,
  metadataDefinitions);
if (!validInput.IsAccepted)
{
  throw new InvalidOperationException("Item input validation rejected a valid selected stack.");
}

ItemInputValidationResult invalidInput = inputValidation.Validate(
  true,
  metadataInventory,
  InventoryComponent.HotbarSlotCount,
  metadataDefinitions);
if (invalidInput.IsAccepted || invalidInput.Rejection.Code != "invalid-slot")
{
  throw new InvalidOperationException("Item input validation accepted a non-hotbar selected slot.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent itemUseTransactionState = new();
ItemUseResult useResult = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25, UseCooldownTicks: 10),
  ref itemUseTransactionState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 1);
if (!useResult.IsAccepted || useResult.Health != 75 || useResult.ConsumedQuantity != 1 ||
    itemUseTransactionState.CooldownTicks != 10)
{
  throw new InvalidOperationException("Item use transaction did not apply recovery and cooldown atomically.");
}

ItemUseResult blockedUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25, UseCooldownTicks: 10),
  ref itemUseTransactionState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 2);
if (blockedUse.IsAccepted || blockedUse.Health != 50)
{
  throw new InvalidOperationException("Item use cooldown allowed a duplicate transaction.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent channelState = new();
ItemUseResult channelUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(
    1,
    99,
    Use: new ItemUseDefinition(
      UseTime: 4,
      UseAnimation: 6,
      Channel: true,
      ManaCost: 3,
      ManaRestore: 1,
      HealthRestore: 5,
      Consumable: true)),
  ref channelState,
  health: 50,
  maximumHealth: 100,
  mana: 5,
  maximumMana: 20,
  sequence: 2);
if (!channelUse.IsAccepted || channelUse.Mana != 3 || channelUse.Event.ManaConsumed != 3 ||
    channelState.AnimationTicks != 6 || !channelState.IsChanneling)
{
  throw new InvalidOperationException(
    "A channelled item did not apply mana cost, recovery and animation state atomically.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent insufficientManaState = new();
ItemUseResult insufficientManaUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(
    1,
    99,
    Use: new ItemUseDefinition(
      UseTime: 1,
      ManaCost: 6,
      ShootType: 1)),
  ref insufficientManaState,
  health: 50,
  maximumHealth: 100,
  mana: 5,
  maximumMana: 20,
  sequence: 3);
if (insufficientManaUse.IsAccepted || insufficientManaState.CooldownTicks != 0)
{
  throw new InvalidOperationException(
    "An item with insufficient mana was accepted or mutated use state.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent recoveryState = new();
ItemUseResult recoveryUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, Recovery: new ItemRecoveryDefinition(Health: 10, Mana: 4)),
  ref recoveryState,
  health: 50,
  maximumHealth: 100,
  mana: 5,
  maximumMana: 20,
  sequence: 4);
if (!recoveryUse.IsAccepted || recoveryUse.Health != 60 || recoveryUse.Mana != 9 ||
    recoveryUse.ConsumedQuantity != 1)
{
  throw new InvalidOperationException(
    "An item recovery definition did not participate in the authoritative use transaction.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent buffRecoveryState = new();
ItemUseResult buffRecoveryUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(
    1,
    99,
    Recovery: new ItemRecoveryDefinition(BuffType: 19, BuffDurationTicks: 60)),
  ref buffRecoveryState,
  health: 100,
  maximumHealth: 100,
  mana: 20,
  maximumMana: 20,
  sequence: 5);
if (!buffRecoveryUse.IsAccepted || buffRecoveryUse.ConsumedQuantity != 1 ||
    buffRecoveryUse.BuffType != 19 || buffRecoveryUse.BuffDurationTicks != 60 ||
    buffRecoveryUse.Event.BuffType != 19 ||
    buffRecoveryUse.Event.BuffDurationTicks != 60)
{
  throw new InvalidOperationException(
    "A buff-only recovery definition did not produce an accepted typed item-use effect.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent invalidBuffRecoveryState = new();
ItemUseResult invalidBuffRecoveryUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(
    1,
    99,
    Recovery: new ItemRecoveryDefinition(BuffType: 19)),
  ref invalidBuffRecoveryState,
  health: 100,
  maximumHealth: 100,
  mana: 20,
  maximumMana: 20,
  sequence: 6);
if (invalidBuffRecoveryUse.IsAccepted || invalidBuffRecoveryState.CooldownTicks != 0)
{
  throw new InvalidOperationException(
    "An invalid buff recovery contract mutated item-use state.");
}

ItemUseCooldownSystem cooldown = new();
for (int tick = 0; tick < 10; tick++)
{
  cooldown.Tick(ref itemUseTransactionState);
}

if (!itemUseTransactionState.CanUse)
{
  throw new InvalidOperationException("Item use cooldown did not expire deterministically.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent forgedUseState = new()
{
  CooldownTicks = -1,
  AnimationTicks = -1,
  IsChanneling = true
};
cooldown.Tick(ref forgedUseState);
if (forgedUseState.CooldownTicks != 0 || forgedUseState.AnimationTicks != 0 ||
    forgedUseState.IsChanneling)
{
  throw new InvalidOperationException(
    "Item use cooldown did not clamp forged negative timers before channel evaluation.");
}

InventoryComponent ammoInventory = new();
ammoInventory.SetSlot(2, new ItemStack(3, 2));
if (!new ItemAmmoConsumptionSystem().TryConsume(ammoInventory, 3, metadataDefinitions, out int ammoSlot) ||
    ammoSlot != 2 || ammoInventory.GetSlot(2) != new ItemStack(3, 1))
{
  throw new InvalidOperationException("Authoritative ammo consumption did not consume one matching stack.");
}

Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent equipmentState;
if (!new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 5),
    existing: null,
    vanity: false,
    out equipmentState,
    out _,
    out _)
    || equipmentState.Slot != ItemEquipmentSlot.Head)
{
  throw new InvalidOperationException("A valid item equipment transaction was rejected.");
}

if (new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 5),
    existing: new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
      ItemEquipmentSlot.Accessory,
      1,
      false),
    vanity: false,
    out _,
    out _,
    out _))
{
  throw new InvalidOperationException("Equipment conflict was accepted.");
}

if (!new ItemPlacementSystem().TryCreateTileCommand(
    metadataDefinitions.Get(5),
    3,
    4,
    1,
    out Terraria.Dome.Simulation.Commands.TileChangeCommand placementCommand,
    out _)
    || placementCommand.TileType != 12)
{
  throw new InvalidOperationException("A valid item placement did not emit a tile command.");
}

if (!new ItemPlacementSystem().TryCreateTileCommand(
    new ItemDefinition(
      6,
      99,
      Placement: new ItemPlacementDefinition(WallType: 7)),
    3,
    4,
    2,
    out Terraria.Dome.Simulation.Commands.TileChangeCommand wallPlacementCommand,
    out _)
    || wallPlacementCommand.Kind != TileChangeKind.SetWall ||
    wallPlacementCommand.WallType != 7)
{
  throw new InvalidOperationException(
    "A valid wall-only item placement did not emit a wall change command.");
}

Console.WriteLine("PASS: item use, cooldown, ammo, equipment and placement systems reject invalid side effects");

ItemDropRuleSystem dropRules = new();
IReadOnlyList<CreateWorldItemCommand> firstDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(3, 1, 3), new ItemDropDefinition(4, 1, 1, ExpertOnly: true)],
  expertMode: true,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
IReadOnlyList<CreateWorldItemCommand> secondDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(3, 1, 3), new ItemDropDefinition(4, 1, 1, ExpertOnly: true)],
  expertMode: true,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
if (firstDrops.Count != 2 || !firstDrops.SequenceEqual(secondDrops))
{
  throw new InvalidOperationException("Item drops were not deterministic for the same seed and tick.");
}

try
{
  _ = dropRules.Evaluate(
    new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
    sourceEntityId: 7,
    tick: 12,
    [new ItemDropDefinition(3)],
    expertMode: false,
    masterMode: false,
    new SimulationVector(float.NaN, 3.0f),
    default,
    spawnSource: 2);
  throw new InvalidOperationException("Item drop rules accepted a non-finite spawn position.");
}
catch (ArgumentOutOfRangeException)
{
}

IReadOnlyList<CreateWorldItemCommand> blockedConditionDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(
    3,
    Conditions: [new ItemDropCondition("Progression", MinimumValue: 2)])],
  expertMode: false,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2,
  conditionValues: new Dictionary<string, int> { ["Progression"] = 1 });
if (blockedConditionDrops.Count != 0)
{
  throw new InvalidOperationException("An unmet item drop condition was accepted.");
}

IReadOnlyList<CreateWorldItemCommand> chainedDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [
    new ItemDropDefinition(3, Weight: 1, ChainId: 4),
    new ItemDropDefinition(4, Weight: 9, ChainId: 4)
  ],
  expertMode: false,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
if (chainedDrops.Count != 1)
{
  throw new InvalidOperationException("A weighted item drop chain did not select exactly one result.");
}

IReadOnlyList<CreateWorldItemCommand> masterExpertDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(3, ExpertOnly: true)],
  expertMode: false,
  masterMode: true,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
if (masterExpertDrops.Count != 1)
{
  throw new InvalidOperationException("Master mode did not satisfy the expert item drop restriction.");
}

ItemDefinition prefixDefinition = new(
  3,
  99,
  Prefixes: new ItemPrefixDefinition([5]));
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent prefixState = new();
if (!new ItemPrefixSystem().TryApply(
    new ItemStack(3, 1),
    prefixDefinition,
    ref prefixState,
    prefixId: 5,
    out _,
    out _)
    || prefixState.PrefixId != 5)
{
  throw new InvalidOperationException("Item prefix application did not produce immutable instance state.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent rejectedPrefixState =
  new(PrefixId: 5);
if (new ItemPrefixSystem().TryApply(
    new ItemStack(3, 1),
    prefixDefinition,
    ref rejectedPrefixState,
    prefixId: 6,
    out _,
    out _) ||
    rejectedPrefixState.PrefixId != 5)
{
  throw new InvalidOperationException(
    "An unregistered prefix mutated direct item instance state.");
}

if (!new ItemPrefixSystem().TryApply(
    new ItemStack(3, 1),
    prefixDefinition,
    ref prefixState,
    prefixId: 0,
    out _,
    out _) ||
    prefixState.PrefixId != 0)
{
  throw new InvalidOperationException("The item prefix reset was not accepted.");
}

ItemDefinition resetOnlyDefinition = new(3, 99);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent resetOnlyState =
  new(PrefixId: 5);
if (!new ItemPrefixSystem().TryApply(
    new ItemStack(3, 1),
    resetOnlyDefinition,
    ref resetOnlyState,
    prefixId: 0,
    out _,
    out _) ||
    resetOnlyState.PrefixId != 0)
{
  throw new InvalidOperationException(
    "A defined item without nonzero prefixes could not clear its existing prefix.");
}

if (!new ItemVariantSystem().TryApply(
    new ItemStack(3, 1),
    ref prefixState,
    new ItemVariantDefinition(1, 3, 4, OneTime: true),
    out ItemStack variantStack,
    out _)
    || variantStack != new ItemStack(4, 1)
    || prefixState.VariantId != 1)
{
  throw new InvalidOperationException("Item variant application did not replace the item type deterministically.");
}

if (new ItemVariantSystem().TryApply(
    variantStack,
    ref prefixState,
    new ItemVariantDefinition(2, 4, 3, OneTime: true),
    out _,
    out _))
{
  throw new InvalidOperationException("One-time item variant was applied twice.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent conditionalVariantState = new();
if (new ItemVariantSystem().TryApply(
    new ItemStack(3, 1),
    ref conditionalVariantState,
    new ItemVariantDefinition(
      3,
      3,
      4,
      Conditions: [new ItemDropCondition("Progression", MinimumValue: 2)]),
    out _,
    out _,
    conditionValues: new Dictionary<string, int> { ["Progression"] = 1 }))
{
  throw new InvalidOperationException("An unmet item variant condition was accepted.");
}

Console.WriteLine("PASS: deterministic item drops, prefixes and one-time variants");

List<ItemInstanceSnapshot> instanceSlots = [
  new ItemInstanceSnapshot(
    new ItemStack(3, 2),
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(PrefixId: 5))];
InventorySnapshot inventorySnapshot = new(new PlayerHandle(1), instanceSlots, 0, 1);
instanceSlots.Clear();
if (inventorySnapshot.Slots.Count != 1 || inventorySnapshot.Slots[0].Stack != new ItemStack(3, 2))
{
  throw new InvalidOperationException("Inventory snapshot did not retain an immutable slot projection.");
}

Terraria.Dome.Simulation.Items.Snapshots.WorldItemSnapshot worldItemSnapshot = new(
  snapshotWorldItem.ReplicationId,
  new ItemInstanceSnapshot(snapshotWorldItem.Stack, snapshotWorldItem.InstanceState),
  snapshotWorldItem.Position,
  snapshotWorldItem.Section,
  snapshotWorldItem.WorldState);
if (worldItemSnapshot.ReplicationId != snapshotWorldItem.ReplicationId ||
    worldItemSnapshot.Instance.Stack != snapshotWorldItem.Stack ||
    worldItemSnapshot.Instance.State != snapshotWorldItem.InstanceState)
{
  throw new InvalidOperationException("World item snapshot lost authoritative identity or stack data.");
}

Console.WriteLine("PASS: immutable item instance, inventory and world-item snapshots");

SelectedItemComponent selection = new();
ItemSelectionSystem selectionSystem = new();
selectionSystem.Apply(inventory, ref selection, 0);
if (selection.SelectedSlot != 0 || selection.Revision != 1)
{
  throw new InvalidOperationException("Selection did not create a deterministic revision.");
}

ItemUseStateComponent useState = new();
PlayerItemUseSystem itemUseSystem = new();
HealthComponent useHealth = new(50, 100);
ItemDefinitionRegistry consumableDefinitions = new([
  new ItemDefinition(1, 99, HealthRestore: 25, UseCooldownTicks: 10)]);
if (!itemUseSystem.TryUse(
  inventory,
  ref useState,
  ref useHealth,
  0,
  consumableDefinitions) || useHealth.Current != 75 || useState.CooldownTicks != 10)
{
  throw new InvalidOperationException("Typed item-use state did not validate and apply a consumable.");
}

Console.WriteLine("PASS: selection and item use have separate typed runtime state");

using DomeSimulation simulation = new(new WorldGrid(400, 300));
PlayerHandle firstPlayer = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
PlayerHandle secondPlayer = simulation.CreatePlayer(new SimulationVector(11.0f, 0.0f));
Arch.Core.QueryDescription playerInventoryQuery = new Arch.Core.QueryDescription()
  .WithAll<Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent>();
int firstPlayerInventoryEntityCount = 0;
bool firstPlayerInventoryComponentAttached = false;
simulation.World.Query(
  in playerInventoryQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player != firstPlayer)
    {
      return;
    }

    firstPlayerInventoryEntityCount++;
    firstPlayerInventoryComponentAttached = simulation.World.Has<InventoryComponent>(entity) &&
      ReferenceEquals(
        simulation.World.Get<InventoryComponent>(entity),
        simulation.GetInventory(firstPlayer));
  });
if (firstPlayerInventoryEntityCount != 1 || !firstPlayerInventoryComponentAttached)
{
  throw new InvalidOperationException(
    "A player inventory was not attached to its authoritative Arch entity.");
}

PlayerHandle replacedInventoryPlayer = simulation.CreatePlayer(new SimulationVector(12.0f, 0.0f));
Arch.Core.Entity replacedInventoryEntity = default;
simulation.World.Query(
  in playerInventoryQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player == replacedInventoryPlayer)
    {
      replacedInventoryEntity = entity;
    }
  });
if (replacedInventoryEntity == default)
{
  throw new InvalidOperationException("Could not locate the player entity for inventory ownership verification.");
}

InventoryComponent replacementInventory = new();
replacementInventory.SetSlot(0, new ItemStack(1, 3));
simulation.World.Set(replacedInventoryEntity, replacementInventory);
if (!ReferenceEquals(simulation.GetInventory(replacedInventoryPlayer), replacementInventory))
{
  throw new InvalidOperationException(
    "GetInventory returned a stale player-handle shadow after the Arch inventory component changed.");
}

int worldItemId = simulation.SpawnWorldItem(new ItemStack(1, 20), new SimulationVector(10.0f, 0.0f));
Arch.Core.QueryDescription worldItemEntityQuery = new Arch.Core.QueryDescription()
  .WithAll<WorldItemComponent>();
int spawnedWorldItemEntityCount = 0;
bool spawnedWorldItemComponentsMatch = false;
simulation.World.Query(
  in worldItemEntityQuery,
  (Arch.Core.Entity entity, ref WorldItemComponent runtimeItem) =>
  {
    if (runtimeItem.ReplicationId != worldItemId)
    {
      return;
    }

    spawnedWorldItemEntityCount++;
    if (!simulation.World.Has<Terraria.Dome.Simulation.Items.Components.ItemStackComponent>(entity) ||
        !simulation.World.Has<Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent>(
          entity) ||
        !simulation.World.Has<Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent>(
          entity) ||
        !simulation.World.Has<Terraria.Dome.Simulation.Items.Components.ItemOwnershipComponent>(
          entity))
    {
      return;
    }

    Terraria.Dome.Simulation.Items.Components.ItemStackComponent stackComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemStackComponent>(entity);
    Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent instanceStateComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent>(entity);
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent worldStateComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent>(entity);
    Terraria.Dome.Simulation.Items.Components.ItemOwnershipComponent ownershipComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemOwnershipComponent>(entity);
    spawnedWorldItemComponentsMatch = stackComponent.Stack == runtimeItem.Stack &&
      instanceStateComponent == runtimeItem.InstanceState &&
      worldStateComponent == runtimeItem.WorldState &&
      ownershipComponent.Player is null && ownershipComponent.ContainerId is null &&
      ownershipComponent.SourceEntityId == runtimeItem.WorldState.SpawnSource &&
      ownershipComponent.OwnershipRevision == runtimeItem.WorldState.LastOwnerRevision;
  });
if (spawnedWorldItemEntityCount != 1 || !spawnedWorldItemComponentsMatch)
{
  throw new InvalidOperationException(
    "A spawned world item did not create synchronized ECS runtime components.");
}

WorldItemCreatedEvent createdEvent = simulation.CreateWorldItemCreatedEvents().Single();
if (createdEvent.ReplicationId != worldItemId ||
    createdEvent.Stack != new ItemStack(1, 20) ||
    createdEvent.Revision != 1)
{
  throw new InvalidOperationException(
    "A committed world-item spawn did not publish its authoritative creation event.");
}

simulation.QueuePickupWorldItem(firstPlayer, worldItemId);
simulation.QueuePickupWorldItem(secondPlayer, worldItemId);
simulation.Tick(new SimulationInputBatch());
if (simulation.GetInventory(firstPlayer).GetSlot(0) != new ItemStack(1, 20) ||
    !simulation.GetInventory(secondPlayer).GetSlot(0).IsEmpty ||
    simulation.CreateWorldItemSnapshots().Single().IsActive)
{
  throw new InvalidOperationException("World item pickup race did not produce one server owner.");
}

WorldItemPickedUpEvent pickupEvent = simulation.CreateWorldItemPickedUpEvents().Single();
if (pickupEvent.ReplicationId != worldItemId || pickupEvent.Player != firstPlayer ||
    pickupEvent.AcceptedQuantity != 20 || pickupEvent.Revision != 2 ||
    simulation.CreateWorldItemCreatedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A committed world-item pickup did not publish only its authoritative pickup event.");
}

using DomeSimulation inactivePickupSimulation = new(new WorldGrid(400, 300));
PlayerHandle inactivePickupPlayer = inactivePickupSimulation.CreatePlayer(
  new SimulationVector(10.0f, 0.0f));
inactivePickupSimulation.QueuePlayerDamage(inactivePickupPlayer, 200);
inactivePickupSimulation.Tick(new SimulationInputBatch());
int inactivePickupItem = inactivePickupSimulation.SpawnWorldItem(
  new ItemStack(1, 1),
  new SimulationVector(10.0f, 0.0f));
inactivePickupSimulation.QueuePickupWorldItem(inactivePickupPlayer, inactivePickupItem);
inactivePickupSimulation.Tick(new SimulationInputBatch());
if (!inactivePickupSimulation.CreateWorldItemSnapshots().Single().IsActive ||
    !inactivePickupSimulation.GetInventory(inactivePickupPlayer).GetSlot(0).IsEmpty ||
    inactivePickupSimulation.CreateWorldItemPickedUpEvents().Count != 0)
{
  throw new InvalidOperationException("An inactive player picked up a world item.");
}

int pickedUpWorldItemEntityCount = 0;
bool pickedUpWorldItemOwnershipMatches = false;
simulation.World.Query(
  in worldItemEntityQuery,
  (Arch.Core.Entity entity, ref WorldItemComponent runtimeItem) =>
  {
    if (runtimeItem.ReplicationId != worldItemId)
    {
      return;
    }

    pickedUpWorldItemEntityCount++;
    Terraria.Dome.Simulation.Items.Components.ItemOwnershipComponent ownershipComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemOwnershipComponent>(entity);
    pickedUpWorldItemOwnershipMatches = ownershipComponent.Player == firstPlayer &&
      ownershipComponent.ContainerId is null &&
      ownershipComponent.OwnershipRevision == runtimeItem.WorldState.LastOwnerRevision;
  });
if (pickedUpWorldItemEntityCount != 1 || !pickedUpWorldItemOwnershipMatches)
{
  throw new InvalidOperationException(
    "A committed pickup did not record the winning player in item ownership state.");
}

InventoryChangedEvent pickupInventoryEvent = simulation.CreateInventoryChangedEvents().Single();
if (pickupInventoryEvent.Player != firstPlayer || pickupInventoryEvent.Slot != 0 ||
    pickupInventoryEvent.Stack != new ItemStack(1, 20) ||
    pickupInventoryEvent.InstanceState != default ||
    pickupInventoryEvent.Revision != simulation.GetInventory(firstPlayer).Revision)
{
  throw new InvalidOperationException(
    "A committed world-item pickup did not publish the changed authoritative inventory slot.");
}

simulation.QueuePickupWorldItem(secondPlayer, worldItemId);
simulation.Tick(new SimulationInputBatch());
if (!simulation.GetInventory(secondPlayer).GetSlot(0).IsEmpty ||
    simulation.CreateWorldItemPickedUpEvents().Count != 0 ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A rejected duplicate pickup created an inventory item or event.");
}

PlayerHandle partialWinner = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
PlayerHandle partialLoser = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
InventoryComponent partialWinnerInventory = simulation.GetInventory(partialWinner);
for (int slot = 0; slot < InventoryComponent.SlotCount; slot++)
{
  partialWinnerInventory.SetSlot(slot, new ItemStack(1, 99));
}

partialWinnerInventory.SetSlot(0, new ItemStack(1, 98));
int partialPickupItemId = simulation.SpawnWorldItem(
  new ItemStack(1, 2),
  new SimulationVector(10.0f, 0.0f));
simulation.QueuePickupWorldItem(partialWinner, partialPickupItemId);
simulation.QueuePickupWorldItem(partialLoser, partialPickupItemId);
simulation.Tick(new SimulationInputBatch());
ItemReplicationSnapshot partialPickupSnapshot = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == partialPickupItemId);
if (partialWinnerInventory.GetSlot(0) != new ItemStack(1, 99) ||
    !simulation.GetInventory(partialLoser).GetSlot(0).IsEmpty ||
    !partialPickupSnapshot.IsActive || partialPickupSnapshot.Stack != new ItemStack(1, 1) ||
    simulation.CreateWorldItemPickedUpEvents().Count != 1)
{
  throw new InvalidOperationException(
    "A partial pickup allowed a second player to win the same world item tick.");
}

int farItemId = simulation.SpawnWorldItem(new ItemStack(1, 1), new SimulationVector(100.0f, 0.0f));
simulation.QueuePickupWorldItem(firstPlayer, farItemId);
simulation.Tick(new SimulationInputBatch());
if (!simulation.GetInventory(firstPlayer).GetSlot(1).IsEmpty ||
    !simulation.CreateWorldItemSnapshots().Single(item => item.ReplicationId == farItemId).IsActive)
{
  throw new InvalidOperationException("Out-of-range pickup mutated authoritative item state.");
}

int delayedSimulationItemId = simulation.SpawnWorldItem(
  new ItemStack(1, 1),
  new SimulationVector(10.0f, 0.0f),
  pickupDelayTicks: 2);
simulation.QueuePickupWorldItem(firstPlayer, delayedSimulationItemId);
simulation.Tick(new SimulationInputBatch());
ItemReplicationSnapshot delayedAfterFirstTick = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == delayedSimulationItemId);
if (!delayedAfterFirstTick.IsActive || delayedAfterFirstTick.WorldState.PickupDelayTicks != 1)
{
  throw new InvalidOperationException(
    "A delayed world item was picked up or did not advance its delay deterministically.");
}

simulation.QueuePickupWorldItem(firstPlayer, delayedSimulationItemId);
simulation.Tick(new SimulationInputBatch());
ItemReplicationSnapshot delayedAfterSecondTick = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == delayedSimulationItemId);
if (!delayedAfterSecondTick.IsActive || delayedAfterSecondTick.WorldState.PickupDelayTicks != 0)
{
  throw new InvalidOperationException(
    "A delayed world item became pickable before its full delay elapsed.");
}

simulation.QueuePickupWorldItem(firstPlayer, delayedSimulationItemId);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateItemReplicationSnapshots()
      .Single(item => item.ReplicationId == delayedSimulationItemId)
      .IsActive)
{
  throw new InvalidOperationException(
    "A world item did not become pickable after its delay elapsed.");
}

int lifecycleWorldItemId = simulation.SpawnWorldItem(
  new ItemStack(1, 3),
  new SimulationVector(10.0f, 0.0f));
simulation.QueueMoveWorldItem(
  lifecycleWorldItemId,
  new SimulationVector(11.0f, 0.0f),
  expectedRevision: 0);
simulation.QueueMoveWorldItem(
  lifecycleWorldItemId,
  new SimulationVector(12.0f, 0.0f),
  expectedRevision: 1);
simulation.Tick(new SimulationInputBatch());
Terraria.Dome.Simulation.WorldItemSnapshot movedLifecycleItem = simulation.CreateWorldItemSnapshots()
  .Single(item => item.ReplicationId == lifecycleWorldItemId);
ItemReplicationSnapshot movedLifecycleReplication = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == lifecycleWorldItemId);
if (!movedLifecycleItem.IsActive || movedLifecycleItem.Position != new SimulationVector(12.0f, 0.0f) ||
    movedLifecycleReplication.Revision != 2 ||
    simulation.CreateWorldItemDestroyedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "World item move commands did not reject stale revisions and commit the valid revision.");
}

simulation.QueueDestroyWorldItem(lifecycleWorldItemId, expectedRevision: 1);
simulation.Tick(new SimulationInputBatch());
if (!simulation.CreateWorldItemSnapshots()
      .Single(item => item.ReplicationId == lifecycleWorldItemId)
      .IsActive || simulation.CreateWorldItemDestroyedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A stale world item destruction command mutated state or published an event.");
}

simulation.QueueDestroyWorldItem(lifecycleWorldItemId, expectedRevision: 2);
simulation.QueuePickupWorldItem(firstPlayer, lifecycleWorldItemId);
simulation.Tick(new SimulationInputBatch());
Terraria.Dome.Simulation.WorldItemSnapshot destroyedLifecycleItem = simulation.CreateWorldItemSnapshots()
  .Single(item => item.ReplicationId == lifecycleWorldItemId);
ItemReplicationSnapshot destroyedLifecycleReplication = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == lifecycleWorldItemId);
WorldItemDestroyedEvent destroyedLifecycleEvent = simulation.CreateWorldItemDestroyedEvents().Single();
if (destroyedLifecycleItem.IsActive || !destroyedLifecycleItem.Stack.IsEmpty ||
    destroyedLifecycleReplication.Revision != 3 ||
    destroyedLifecycleEvent != new WorldItemDestroyedEvent(lifecycleWorldItemId, 3) ||
    simulation.CreateWorldItemPickedUpEvents().Count != 0)
{
  throw new InvalidOperationException(
    "An accepted world item destruction did not create a tombstone before pickup resolution.");
}

int destroyedWorldItemEntityCount = 0;
bool destroyedWorldItemComponentsMatch = false;
simulation.World.Query(
  in worldItemEntityQuery,
  (Arch.Core.Entity entity, ref WorldItemComponent runtimeItem) =>
  {
    if (runtimeItem.ReplicationId != lifecycleWorldItemId)
    {
      return;
    }

    destroyedWorldItemEntityCount++;
    Terraria.Dome.Simulation.Items.Components.ItemStackComponent stackComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemStackComponent>(entity);
    Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent instanceStateComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent>(entity);
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent worldStateComponent =
      simulation.World.Get<Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent>(entity);
    destroyedWorldItemComponentsMatch = stackComponent.Stack.IsEmpty &&
      instanceStateComponent == default && !worldStateComponent.IsActive &&
      runtimeItem.Stack.IsEmpty && !runtimeItem.IsActive &&
      worldStateComponent == runtimeItem.WorldState;
  });
if (destroyedWorldItemEntityCount != 1 || !destroyedWorldItemComponentsMatch)
{
  throw new InvalidOperationException(
    "A destroyed world item did not synchronize its ECS tombstone components.");
}

int metadataWorldItemId = simulation.SpawnWorldItem(
  new ItemStack(1, 1),
  new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
    PrefixId: 8,
    IsFavorited: true),
  new SimulationVector(12.0f, 0.0f));
ItemReplicationSnapshot metadataWorldItem = simulation.CreateItemReplicationSnapshots()
  .Single(item => item.ReplicationId == metadataWorldItemId);
if (metadataWorldItem.InstanceState !=
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      PrefixId: 8,
      IsFavorited: true))
{
  throw new InvalidOperationException(
    "The simulation item replication snapshot discarded world item instance state.");
}

Console.WriteLine("PASS: server-owned pickup race and duplicate pickup rejection");

simulation.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 1)));
if (simulation.GetInventory(firstPlayer).SelectedSlot != 1)
{
  throw new InvalidOperationException("A valid selected inventory slot was not applied by simulation.");
}

simulation.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: InventoryComponent.SlotCount)));
if (simulation.GetInventory(firstPlayer).SelectedSlot != 1)
{
  throw new InvalidOperationException("An invalid selected slot changed server-owned inventory state.");
}

Console.WriteLine("PASS: selected item assertions are range-validated by the server");

simulation.GetInventory(firstPlayer).SetSlot(1, new ItemStack(1, 2));
simulation.QueuePlayerDamage(firstPlayer, 50);
simulation.Tick(new SimulationInputBatch());
int healthBeforeUse = simulation.CreateSnapshot().FindPlayer(firstPlayer).Health;
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 1,
  UseItem: true)));
if (simulation.CreateSnapshot().FindPlayer(firstPlayer).Health <= healthBeforeUse ||
    simulation.GetInventory(firstPlayer).GetSlot(1) != new ItemStack(1, 1))
{
  throw new InvalidOperationException("A selected consumable did not apply its server-owned use effect.");
}

ItemUsedEvent useEvent = simulation.CreateItemUsedEvents().Single();
if (useEvent.Player != firstPlayer || useEvent.ItemType != 1 || useEvent.Slot != 1 ||
    useEvent.HealthRestored <= 0 || !useEvent.ConsumedMainItem)
{
  throw new InvalidOperationException(
    "A committed item use did not publish its authoritative use event.");
}

InventoryChangedEvent useInventoryEvent = simulation.CreateInventoryChangedEvents().Single();
if (useInventoryEvent.Player != firstPlayer || useInventoryEvent.Slot != 1 ||
    useInventoryEvent.Stack != new ItemStack(1, 1) ||
    useInventoryEvent.Revision != simulation.GetInventory(firstPlayer).Revision)
{
  throw new InvalidOperationException(
    "A committed consumable use did not publish its authoritative inventory change.");
}

int healthAfterUse = simulation.CreateSnapshot().FindPlayer(firstPlayer).Health;
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  firstPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 1,
  UseItem: true)));
if (simulation.CreateSnapshot().FindPlayer(firstPlayer).Health != healthAfterUse ||
    simulation.GetInventory(firstPlayer).GetSlot(1) != new ItemStack(1, 1) ||
    simulation.CreateItemUsedEvents().Count != 0 ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "Item use cooldown allowed a duplicate use or event on the next tick.");
}

Console.WriteLine("PASS: selected item use is server-owned, consumable and cooldown-limited");

PlayerHandle buffPlayer = simulation.CreatePlayer(new SimulationVector(25.0f, 0.0f));
InventoryComponent buffInventory = simulation.GetInventory(buffPlayer);
buffInventory.SetSlot(0, new ItemStack(7, 1));
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  buffPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 0,
  UseItem: true)));
Arch.Core.QueryDescription buffQuery = new Arch.Core.QueryDescription()
  .WithAll<Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent>();
bool buffWasApplied = false;
simulation.World.Query(
  in buffQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player != buffPlayer)
    {
      return;
    }

    BuffCollectionComponent buffs = simulation.World.Get<BuffCollectionComponent>(entity);
    buffWasApplied = buffs.Entries.Count == 1 &&
      buffs.Entries[0] == new BuffEntry(19, 60, buffPlayer);
  });
ItemUsedEvent buffEvent = simulation.CreateItemUsedEvents().Single();
if (!buffWasApplied || !buffInventory.GetSlot(0).IsEmpty ||
    buffEvent.ItemType != 7 || buffEvent.BuffType != 19 ||
    buffEvent.BuffDurationTicks != 60 ||
    simulation.CreateInventoryChangedEvents().Count != 1)
{
  throw new InvalidOperationException(
    "A committed buff-only item use did not apply its typed buff atomically.");
}

Console.WriteLine("PASS: buff-only recovery items apply server-owned duration state");

PlayerHandle combatProjectilePlayer = simulation.CreatePlayer(new SimulationVector(27.0f, 0.0f));
InventoryComponent combatProjectileInventory = simulation.GetInventory(combatProjectilePlayer);
combatProjectileInventory.SetSlot(0, new ItemStack(8, 1));
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  combatProjectilePlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 0,
  UseItem: true)));
ProjectileReplicationSnapshot combatProjectile = simulation.CreateProjectileReplicationSnapshots()
  .SingleOrDefault(projectile => projectile.Owner == combatProjectilePlayer && projectile.IsActive);
if (combatProjectile.ReplicationId == 0 || combatProjectile.ProjectileType != 2 ||
    combatProjectile.Damage != 14 || MathF.Abs(combatProjectile.Velocity.X - 5.0f) > 0.001f)
{
  throw new InvalidOperationException(
    "A combat-defined projectile item did not use its damage, type and speed metadata.");
}

Console.WriteLine("PASS: combat projectile definitions fallback into item use");

PlayerHandle ammunitionPlayer = simulation.CreatePlayer(new SimulationVector(30.0f, 0.0f));
InventoryComponent ammunitionInventory = simulation.GetInventory(ammunitionPlayer);
ammunitionInventory.SetSlot(0, new ItemStack(2, 1));
ammunitionInventory.SetSlot(1, new ItemStack(3, 2));
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  ammunitionPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 0,
  UseItem: true)));
if (ammunitionInventory.GetSlot(0) != new ItemStack(2, 1) ||
    ammunitionInventory.GetSlot(1) != new ItemStack(3, 1) ||
    simulation.CreateItemUseSnapshot(ammunitionPlayer).State.CooldownTicks <= 0 ||
    simulation.CreateItemUsedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents()[0].Slot != 1)
{
  throw new InvalidOperationException(
    "A server-authoritative ranged item did not consume exactly one matching ammunition stack.");
}

ProjectileReplicationSnapshot itemProjectile = simulation.CreateProjectileReplicationSnapshots()
  .SingleOrDefault(projectile => projectile.Owner == ammunitionPlayer && projectile.IsActive);
if (itemProjectile.ReplicationId == 0 || itemProjectile.ProjectileType != 2 ||
    MathF.Abs(itemProjectile.Velocity.X - 6.0f) > 0.001f)
{
  throw new InvalidOperationException(
    "A ranged item use did not spawn its definition-selected projectile and speed.");
}

PlayerHandle noAmmunitionPlayer = simulation.CreatePlayer(new SimulationVector(31.0f, 0.0f));
InventoryComponent noAmmunitionInventory = simulation.GetInventory(noAmmunitionPlayer);
noAmmunitionInventory.SetSlot(0, new ItemStack(2, 1));
simulation.Tick(new SimulationInputBatch(new PlayerInput(
  noAmmunitionPlayer,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  Fire: false,
  SelectedSlot: 0,
  UseItem: true)));
if (noAmmunitionInventory.GetSlot(0) != new ItemStack(2, 1) ||
    simulation.CreateItemUseSnapshot(noAmmunitionPlayer).State.CooldownTicks != 0 ||
    simulation.CreateItemUsedEvents().Count != 0 ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A no-ammunition item use mutated authoritative runtime state.");
}

Console.WriteLine("PASS: selected ranged item use is ammunition-gated and server-owned");

PlayerHandle placementPlayer = simulation.CreatePlayer(new SimulationVector(40.0f, 0.0f));
InventoryComponent placementInventory = simulation.GetInventory(placementPlayer);
placementInventory.SetSlot(0, new ItemStack(5, 2));
simulation.QueuePlaceItem(placementPlayer, 0, 40, 1);
simulation.Tick(new SimulationInputBatch());
if (simulation.WorldGrid.GetTile(40, 1) != new WorldTile(IsActive: true, Type: 12) ||
    placementInventory.GetSlot(0) != new ItemStack(5, 1) ||
    simulation.CreateInventoryChangedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents()[0].Slot != 0)
{
  throw new InvalidOperationException(
    "A valid item placement did not commit its tile change and one-item consumption.");
}

simulation.QueuePlaceItem(placementPlayer, 0, 40, 1);
simulation.Tick(new SimulationInputBatch());
if (placementInventory.GetSlot(0) != new ItemStack(5, 1))
{
  throw new InvalidOperationException(
    "Placement on an occupied tile caused an unauthorized inventory mutation.");
}

placementInventory.SetSlot(1, new ItemStack(6, 1));
simulation.QueuePlaceItem(placementPlayer, 1, 40, 1);
simulation.Tick(new SimulationInputBatch());
if (simulation.WorldGrid.GetTile(40, 1) !=
      new WorldTile(IsActive: true, Type: 12, WallType: 7) ||
    !placementInventory.GetSlot(1).IsEmpty ||
    simulation.CreateInventoryChangedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents()[0].Slot != 1)
{
  throw new InvalidOperationException(
    "A wall-only item did not commit its wall change on an active tile.");
}

placementInventory.SetSlot(1, new ItemStack(6, 1));
simulation.QueuePlaceItem(placementPlayer, 1, 40, 1);
simulation.Tick(new SimulationInputBatch());
if (placementInventory.GetSlot(1) != new ItemStack(6, 1) ||
    simulation.WorldGrid.GetTile(40, 1).WallType != 7)
{
  throw new InvalidOperationException(
    "Wall placement on an occupied wall mutated inventory state.");
}

Console.WriteLine(
  "PASS: item placement is range-checked, tile/wall-committed and consumable");

PlayerHandle equipmentPlayer = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
InventoryComponent equipmentInventory = simulation.GetInventory(equipmentPlayer);
equipmentInventory.SetSlot(0, new ItemStack(4, 1));
simulation.QueueEquipItem(equipmentPlayer, 0, isVanity: false);
simulation.Tick(new SimulationInputBatch());
EquipmentSnapshot runtimeEquipmentSnapshot = simulation.CreateEquipmentSnapshot(equipmentPlayer);
if (runtimeEquipmentSnapshot.Slots.Count != 1 ||
    runtimeEquipmentSnapshot.Slots[0] !=
      new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
        ItemEquipmentSlot.Head,
        0,
        false))
{
  throw new InvalidOperationException("A valid equipment command did not create authoritative state.");
}

Arch.Core.QueryDescription equipmentStateQuery = new Arch.Core.QueryDescription()
  .WithAll<Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent>();
int equipmentStateEntityCount = 0;
bool equipmentStateComponentAttached = false;
bool equipmentStateSnapshotMatches = false;
simulation.World.Query(
  in equipmentStateQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player != equipmentPlayer)
    {
      return;
    }

    equipmentStateEntityCount++;
    if (!simulation.World.Has<EquipmentStateCollectionComponent>(entity))
    {
      return;
    }

    EquipmentStateCollectionComponent states =
      simulation.World.Get<EquipmentStateCollectionComponent>(entity);
    equipmentStateComponentAttached = true;
    equipmentStateSnapshotMatches = states.States.Count == runtimeEquipmentSnapshot.Slots.Count &&
      states.States[ItemEquipmentSlot.Head] == runtimeEquipmentSnapshot.Slots[0] &&
      states.Revision == runtimeEquipmentSnapshot.Revision;
  });
if (equipmentStateEntityCount != 1 || !equipmentStateComponentAttached ||
    !equipmentStateSnapshotMatches)
{
  throw new InvalidOperationException(
    "Equipment state was not attached to the player Arch entity or snapshot projection diverged.");
}

int equippedDefense = -1;
simulation.World.Query(
  in equipmentStateQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player != equipmentPlayer)
    {
      return;
    }

    equippedDefense = simulation.World.Get<
      Terraria.Dome.Simulation.Combat.Components.DefenseComponent>(entity).Value;
  });
if (equippedDefense != 5)
{
  throw new InvalidOperationException(
    "An equipped definition did not apply its server-owned defense to the player entity.");
}

ItemEquippedEvent equippedEvent = simulation.CreateItemEquippedEvents().Single();
if (equippedEvent.Player != equipmentPlayer || equippedEvent.ItemType != 4 ||
    equippedEvent.Slot != ItemEquipmentSlot.Head || equippedEvent.SourceSlot != 0 ||
    equippedEvent.IsVanity)
{
  throw new InvalidOperationException(
    "A committed equipment command did not publish its authoritative event.");
}

simulation.QueuePlayerDamage(equipmentPlayer, 10);
simulation.Tick(new SimulationInputBatch());
int healthAfterEquippedDamage = simulation.CreatePlayerStateSnapshot(equipmentPlayer).Health;
for (int index = 0; index < 4; index++)
{
  simulation.Tick(new SimulationInputBatch());
}

if (simulation.CreatePlayerStateSnapshot(equipmentPlayer).Health != healthAfterEquippedDamage + 2)
{
  throw new InvalidOperationException(
    "An equipped life-regeneration definition did not accelerate authoritative recovery.");
}

equipmentInventory.SetSlot(1, new ItemStack(4, 1));
simulation.QueueEquipItem(equipmentPlayer, 1, isVanity: false);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateEquipmentSnapshot(equipmentPlayer).Slots[0].SourceSlot != 0)
{
  throw new InvalidOperationException("An occupied equipment slot was replaced without an unequip command.");
}
if (simulation.CreateItemEquippedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "An occupied equipment slot rejection published an equipment event.");
}

simulation.QueueUnequipItem(equipmentPlayer, ItemEquipmentSlot.Head);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateEquipmentSnapshot(equipmentPlayer).Slots.Count != 0)
{
  throw new InvalidOperationException("An authoritative unequip command did not clear the equipment slot.");
}
int unequippedDefense = -1;
simulation.World.Query(
  in equipmentStateQuery,
  (Arch.Core.Entity entity,
    ref Terraria.Dome.Simulation.Player.Components.PlayerIdentityComponent identity) =>
  {
    if (identity.Player != equipmentPlayer)
    {
      return;
    }

    unequippedDefense = simulation.World.Get<
      Terraria.Dome.Simulation.Combat.Components.DefenseComponent>(entity).Value;
  });
if (unequippedDefense != 0)
{
  throw new InvalidOperationException(
    "Unequip did not remove the server-owned defense contribution.");
}

simulation.QueuePlayerDamage(equipmentPlayer, 10);
simulation.Tick(new SimulationInputBatch());
int healthAfterUnequippedDamage = simulation.CreatePlayerStateSnapshot(equipmentPlayer).Health;
for (int index = 0; index < 3; index++)
{
  simulation.Tick(new SimulationInputBatch());
}

if (simulation.CreatePlayerStateSnapshot(equipmentPlayer).Health != healthAfterUnequippedDamage)
{
  throw new InvalidOperationException(
    "Unequip did not remove the server-owned life-regeneration contribution.");
}
if (simulation.CreateItemEquippedEvents().Count != 0)
{
  throw new InvalidOperationException("Unequip unexpectedly published an equip event.");
}

simulation.QueueEquipItem(equipmentPlayer, 1, isVanity: false);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateEquipmentSnapshot(equipmentPlayer).Slots.Count != 1 ||
    simulation.CreateEquipmentSnapshot(equipmentPlayer).Slots[0].SourceSlot != 1)
{
  throw new InvalidOperationException("An unequipped slot did not accept a later equipment command.");
}
if (simulation.CreateItemEquippedEvents().Count != 1 ||
    simulation.CreateItemEquippedEvents()[0].SourceSlot != 1)
{
  throw new InvalidOperationException(
    "A later equipment command did not publish its committed event.");
}

Console.WriteLine("PASS: equipment commands create immutable server-owned snapshots");

InventorySnapshot runtimeInventorySnapshot = simulation.CreateInventorySnapshot(firstPlayer);
if (runtimeInventorySnapshot.Player != firstPlayer ||
    runtimeInventorySnapshot.Slots.Count != InventoryComponent.SlotCount ||
    runtimeInventorySnapshot.SelectedSlot != 1 ||
    runtimeInventorySnapshot.Revision != simulation.GetInventory(firstPlayer).Revision ||
    runtimeInventorySnapshot.Slots[1].Stack != new ItemStack(1, 1))
{
  throw new InvalidOperationException(
    "The immutable inventory snapshot did not project authoritative runtime state.");
}

ItemUseSnapshot runtimeUseSnapshot = simulation.CreateItemUseSnapshot(firstPlayer);
if (runtimeUseSnapshot.Player != firstPlayer ||
    runtimeUseSnapshot.State.CooldownTicks < 0 ||
    runtimeUseSnapshot.Revision != simulation.TickNumber)
{
  throw new InvalidOperationException(
    "The immutable item-use snapshot did not project the authoritative cooldown.");
}

Console.WriteLine("PASS: simulation publishes immutable inventory and item-use snapshots");

PlayerHandle modifierPlayer = simulation.CreatePlayer(new SimulationVector(60.0f, 0.0f));
InventoryComponent modifierInventory = simulation.GetInventory(modifierPlayer);
modifierInventory.SetSlot(0, new ItemStack(3, 1));
simulation.QueueApplyItemPrefix(modifierPlayer, 0, prefixId: 7);
simulation.Tick(new SimulationInputBatch());
if (modifierInventory.GetInstanceState(0).PrefixId != 7)
{
  throw new InvalidOperationException(
    "The authoritative prefix command did not update inventory state.");
}
ItemPrefixChangedEvent prefixEvent = simulation.CreateItemPrefixChangedEvents().Single();
if (prefixEvent.ItemType != 3 || prefixEvent.PreviousPrefixId != 0 || prefixEvent.PrefixId != 7)
{
  throw new InvalidOperationException(
    "A committed prefix command did not publish its authoritative event.");
}
if (simulation.CreateInventoryChangedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents()[0].InstanceState.PrefixId != 7)
{
  throw new InvalidOperationException(
    "A committed prefix command did not publish the changed inventory instance state.");
}

long prefixRevisionBeforeRejection = modifierInventory.Revision;
simulation.QueueApplyItemPrefix(modifierPlayer, 0, prefixId: 8);
simulation.Tick(new SimulationInputBatch());
if (modifierInventory.GetInstanceState(0).PrefixId != 7 ||
    modifierInventory.Revision != prefixRevisionBeforeRejection ||
    simulation.CreateItemPrefixChangedEvents().Count != 0 ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "An unregistered prefix command mutated authoritative inventory state.");
}

simulation.QueueApplyItemPrefix(modifierPlayer, 2, prefixId: 8);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateItemPrefixChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A rejected prefix command published an authoritative event.");
}
if (simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A rejected prefix command published an inventory change event.");
}

simulation.QueueApplyItemVariant(
  modifierPlayer,
  0,
  new ItemVariantDefinition(7, 3, 5, OneTime: true));
simulation.Tick(new SimulationInputBatch());
if (modifierInventory.GetSlot(0) != new ItemStack(5, 1) ||
    modifierInventory.GetInstanceState(0).VariantId != 7 ||
    simulation.CreateItemPrefixChangedEvents().Count != 0 ||
    simulation.CreateInventoryChangedEvents().Count != 1 ||
    simulation.CreateInventoryChangedEvents()[0].InstanceState.VariantId != 7)
{
  throw new InvalidOperationException(
    "The authoritative variant command did not update item state.");
}

simulation.QueueApplyItemVariant(
  modifierPlayer,
  0,
  new ItemVariantDefinition(8, 5, 3, OneTime: true));
simulation.Tick(new SimulationInputBatch());
if (modifierInventory.GetSlot(0) != new ItemStack(5, 1) ||
    modifierInventory.GetInstanceState(0).VariantId != 7)
{
  throw new InvalidOperationException(
    "A one-time variant command mutated item state after rejection.");
}

Console.WriteLine("PASS: prefix and variant commands are authoritative and one-time guarded");

PlayerHandle inventoryCommandPlayer = simulation.CreatePlayer(new SimulationVector(90.0f, 0.0f));
InventoryComponent commandInventory = simulation.GetInventory(inventoryCommandPlayer);
commandInventory.SetSlot(0, new ItemStack(1, 5));
simulation.QueueSplitItemStack(inventoryCommandPlayer, 0, 1, 2);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(0) != new ItemStack(1, 3) ||
    commandInventory.GetSlot(1) != new ItemStack(1, 2) ||
    simulation.CreateInventoryChangedEvents().Count != 2)
{
  throw new InvalidOperationException(
    "A committed split command did not preserve both stack quantities or events.");
}

simulation.QueueMergeItemStack(inventoryCommandPlayer, 1, 0, 2);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(0) != new ItemStack(1, 5) ||
    !commandInventory.GetSlot(1).IsEmpty ||
    simulation.CreateInventoryChangedEvents().Count != 2)
{
  throw new InvalidOperationException(
    "A committed merge command did not combine compatible item instances atomically.");
}

commandInventory.SetSlot(0, new ItemStack(1, 4));
commandInventory.SetSlot(1, new ItemStack(1, 98));
simulation.QueueTransferItem(inventoryCommandPlayer, 0, 1, 2);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(0) != new ItemStack(1, 4) ||
    commandInventory.GetSlot(1) != new ItemStack(1, 98) ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "An over-capacity transfer partially mutated inventory state.");
}

simulation.QueueTransferItem(inventoryCommandPlayer, 0, 1, 1);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(0) != new ItemStack(1, 3) ||
    commandInventory.GetSlot(1) != new ItemStack(1, 99) ||
    simulation.CreateInventoryChangedEvents().Count != 2)
{
  throw new InvalidOperationException(
    "A valid transfer did not merge the exact requested quantity.");
}

commandInventory.SetSlot(2, new ItemStack(3, 2));
simulation.QueueTransferItem(inventoryCommandPlayer, 2, 1, 1);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(2) != new ItemStack(3, 2) ||
    commandInventory.GetSlot(1) != new ItemStack(1, 99) ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "An incompatible transfer mutated inventory state or published an event.");
}

simulation.QueueDropItem(inventoryCommandPlayer, 0, 2);
simulation.Tick(new SimulationInputBatch());
ItemDroppedEvent playerDropEvent = simulation.CreateItemDroppedEvents().Single();
WorldItemCreatedEvent playerDropCreatedEvent = simulation.CreateWorldItemCreatedEvents().Single();
Terraria.Dome.Simulation.WorldItemSnapshot playerDropWorldItem = simulation.CreateWorldItemSnapshots()
  .Single(item => item.ReplicationId == playerDropCreatedEvent.ReplicationId);
if (commandInventory.GetSlot(0) != new ItemStack(1, 1) ||
    playerDropEvent.SourceEntityId != inventoryCommandPlayer.Value ||
    playerDropEvent.ItemType != 1 || playerDropEvent.Quantity != 2 ||
    playerDropEvent.Tick != simulation.TickNumber ||
    !playerDropWorldItem.IsActive || playerDropWorldItem.Stack != new ItemStack(1, 2) ||
    simulation.CreateInventoryChangedEvents().Count != 1)
{
  throw new InvalidOperationException(
    "A committed drop did not consume inventory and publish drop/create/change facts.");
}

Console.WriteLine("PASS: transfer, split, merge and drop commands are atomic and authoritative");

NpcHandle lootNpc = simulation.CreateNpc(new SimulationVector(70.0f, 0.0f));
simulation.QueueNpcDamage(lootNpc, 100);
simulation.Tick(new SimulationInputBatch());
ItemDroppedEvent droppedEvent = simulation.CreateItemDroppedEvents().Single();
if (droppedEvent.SourceEntityId != lootNpc.Value || droppedEvent.ItemType != 1 ||
    droppedEvent.Quantity < 1 || droppedEvent.Quantity > 2 ||
    droppedEvent.Tick != simulation.TickNumber ||
    simulation.CreateWorldItemCreatedEvents().Count != 1)
{
  throw new InvalidOperationException(
    "Committed NPC loot did not publish drop and world-item creation events.");
}

Console.WriteLine("PASS: committed item events publish only accepted use, pickup, equipment, prefix and drop facts");

PlayerPersistentState importedAccount = CreatePersistentState(
  "2eecdeea-c45e-456f-8244-75ec32da6172",
  firstItemType: 1,
  firstItemStack: 7,
  retainedItemType: 999,
  retainedItemStack: 23);
PlayerPersistentState conflictingAccount = CreatePersistentState(
  "2eecdeea-c45e-456f-8244-75ec32da6172",
  firstItemType: 1,
  firstItemStack: 3,
  retainedItemType: 777,
  retainedItemStack: 1);

PlayerPersistentState importedResult = simulation.ImportPlayerIfMissing(importedAccount);
PlayerHandle importedPlayer = simulation.CreatePlayer(importedResult, new SimulationVector(20.0f, 0.0f));
bool foundStoredAccount = simulation.TryGetPlayerPersistentState(importedAccount.Uuid, out PlayerPersistentState? stored);
if (simulation.GetInventory(importedPlayer).GetSlot(0) != new ItemStack(1, 7) ||
    simulation.GetInventory(importedPlayer).GetInstanceState(0) !=
      new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
        PrefixId: 11,
        IsFavorited: true,
        IsNewAndShiny: true) ||
    !foundStoredAccount || stored is null ||
    stored.Items.Count != PlayerPersistentState.ItemSlotCount ||
    stored.Items[10] != new PlayerPersistentItem(10, 23, 0, 999, false, false))
{
  throw new InvalidOperationException(
    "First account import did not project the hotbar and retain all original item slots.");
}

PlayerPersistentState existingResult = simulation.ImportPlayerIfMissing(conflictingAccount);
if (existingResult.Items[0].Stack != 7 || existingResult.Items[10].ItemType != 999 ||
    simulation.GetInventory(importedPlayer).GetSlot(0) != new ItemStack(1, 7))
{
  throw new InvalidOperationException(
    "A later bootstrap state replaced the server-owned player account.");
}

Console.WriteLine("PASS: UUID player accounts import once and retain complete original inventory state");

simulation.GetInventory(importedPlayer).SetSlot(0, new ItemStack(1, 4));
simulation.GetInventory(importedPlayer).SetInstanceState(
  0,
  new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
    PrefixId: 17,
    IsFavorited: false));
DomeSimulationSnapshot persistedSimulation = simulation.CreatePersistenceSnapshot(
  new WorldMetadata("Account update", new WorldSeed(1), 400, 300));
if (persistedSimulation.PlayerAccounts.Count != 1 ||
    persistedSimulation.PlayerAccounts[0].Items[0].Stack != 4 ||
    persistedSimulation.PlayerAccounts[0].Items[0].Prefix != 17 ||
    persistedSimulation.PlayerAccounts[0].Items[0].IsFavorited ||
    persistedSimulation.PlayerAccounts[0].Items[0].IsNewAndShiny ||
    persistedSimulation.PlayerAccounts[0].Items[10].ItemType != 999 ||
    persistedSimulation.PlayerAccounts[0].Items[10].Stack != 23)
{
  throw new InvalidOperationException(
    "A server-owned runtime inventory update was not merged into the persistent account.");
}

simulation.GetInventory(importedPlayer).SetInstanceState(
  0,
  new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(PrefixId: 256));
bool rejectedUnrepresentablePrefix = false;
try
{
  _ = simulation.CreatePersistenceSnapshot(
    new WorldMetadata("Unrepresentable prefix", new WorldSeed(1), 400, 300));
}
catch (InvalidOperationException)
{
  rejectedUnrepresentablePrefix = true;
}

if (!rejectedUnrepresentablePrefix)
{
  throw new InvalidOperationException(
    "Persistence accepted an item prefix outside the V1456 byte representation.");
}

Console.WriteLine("PASS: runtime inventory projection updates only its persistent hotbar slots");

ItemDefinition legacyDefinition = LegacyItemDefinitionAdapter.ToDefinition(
  new LegacyItemDefinitionRecord(
    ItemType: 6,
    MaxStack: 20,
    HealthRestore: 15,
    UseCooldownTicks: 4,
    NameKey: "Legacy.Item"));
if (legacyDefinition.ItemType != 6 || legacyDefinition.StackLimit != 20 ||
    legacyDefinition.HealthRestore != 15 || legacyDefinition.UseCooldownTicks != 4 ||
    legacyDefinition.Identity?.NameKey != "Legacy.Item")
{
  throw new InvalidOperationException(
    "Legacy item definition data was not mapped to ECS metadata.");
}

if (!LegacyItemImportAdapter.TryImport(
      new PlayerPersistentItem(
        10,
        3,
        11,
        6,
        true,
        true,
        VariantId: 4,
        Dye: 5,
        Paint: 6,
        NameOverride: "Imported"),
      legacyDefinition,
      out Terraria.Dome.Simulation.Items.Snapshots.ItemInstanceSnapshot importedItem) ||
    importedItem.Stack != new ItemStack(6, 3) ||
    importedItem.State != new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      PrefixId: 11,
      VariantId: 4,
      Dye: 5,
      Paint: 6,
      IsFavorited: true,
      IsNewAndShiny: true,
      NameOverride: "Imported"))
{
  throw new InvalidOperationException(
    "Legacy player item data was not imported as instance state.");
}

LegacyItemDropRecord legacyDrop = new(6, 1, 2, PrefixId: 9, VariantId: 3);
if (!LegacyItemDropAdapter.TryCreateCommand(
      legacyDrop,
      quantity: 2,
      new SimulationVector(3.0f, 4.0f),
      new WorldSectionCoordinates(0, 0),
      spawnSource: 1,
      out CreateWorldItemCommand dropCommand) ||
    dropCommand.Stack != new ItemStack(6, 2) ||
    dropCommand.InstanceState.PrefixId != 9 ||
    dropCommand.InstanceState.VariantId != 3)
{
  throw new InvalidOperationException(
    "Legacy item drop data was not mapped to a world-item command.");
}

Console.WriteLine("PASS: legacy item compatibility adapters map data at the boundary");

using (DomeSimulation extractinatorSimulation = new(new WorldGrid(400, 300)))
{
  PlayerHandle extractinatorPlayer = extractinatorSimulation.CreatePlayer(
    new SimulationVector(100.0f, 0.0f));
  InventoryComponent extractinatorInventory = extractinatorSimulation.GetInventory(extractinatorPlayer);
  extractinatorInventory.SetSlot(0, new ItemStack(5395, 2));
  _ = extractinatorSimulation.WorldGrid.TrySetTile(
    100,
    1,
    new WorldTile(IsActive: true, Type: ExtractinatorSystem.ExtractinatorTileType));

  extractinatorSimulation.QueueUseExtractinator(extractinatorPlayer, 0, 100, 1);
  extractinatorSimulation.Tick(new SimulationInputBatch());
  if (extractinatorInventory.GetSlot(0) != new ItemStack(5395, 1) ||
      extractinatorSimulation.CreateExtractinatorResultEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A valid Extractinator use did not consume one input and publish one committed result.");
  }

  long extractinatorInventoryRevision = extractinatorInventory.Revision;
  extractinatorSimulation.QueueUseExtractinator(extractinatorPlayer, 0, 101, 1);
  extractinatorSimulation.Tick(new SimulationInputBatch());
  if (extractinatorInventory.GetSlot(0) != new ItemStack(5395, 1) ||
      extractinatorInventory.Revision != extractinatorInventoryRevision ||
      extractinatorSimulation.CreateExtractinatorResultEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "An invalid Extractinator target mutated authoritative inventory or events.");
  }

  _ = extractinatorSimulation.WorldGrid.TrySetTile(
    113,
    1,
    new WorldTile(IsActive: true, Type: ExtractinatorSystem.ExtractinatorTileType));
  extractinatorSimulation.QueueUseExtractinator(extractinatorPlayer, 0, 113, 1);
  extractinatorSimulation.Tick(new SimulationInputBatch());
  if (extractinatorInventory.GetSlot(0) != new ItemStack(5395, 1) ||
      extractinatorSimulation.CreateExtractinatorResultEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "An out-of-range Extractinator target mutated authoritative state.");
  }
}

using (DomeSimulation duplicateExtractinatorSimulation = new(new WorldGrid(400, 300)))
{
  PlayerHandle player = duplicateExtractinatorSimulation.CreatePlayer(
    new SimulationVector(100.0f, 0.0f));
  InventoryComponent duplicateInventory = duplicateExtractinatorSimulation.GetInventory(player);
  duplicateInventory.SetSlot(0, new ItemStack(5395, 2));
  _ = duplicateExtractinatorSimulation.WorldGrid.TrySetTile(
    100,
    1,
    new WorldTile(IsActive: true, Type: ExtractinatorSystem.ExtractinatorTileType));
  duplicateExtractinatorSimulation.QueueUseExtractinator(player, 0, 100, 1);
  duplicateExtractinatorSimulation.QueueUseExtractinator(player, 0, 100, 1);
  duplicateExtractinatorSimulation.Tick(new SimulationInputBatch());
  if (duplicateInventory.GetSlot(0) != new ItemStack(5395, 1) ||
      duplicateExtractinatorSimulation.CreateExtractinatorResultEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "Same-tick Extractinator intents did not select exactly one successful command.");
  }
}

using (DomeSimulation fullInventoryExtractinatorSimulation = new(new WorldGrid(400, 300)))
{
  PlayerHandle player = fullInventoryExtractinatorSimulation.CreatePlayer(
    new SimulationVector(100.0f, 0.0f));
  InventoryComponent fullInventory = fullInventoryExtractinatorSimulation.GetInventory(player);
  for (int slot = 0; slot < InventoryComponent.SlotCount; slot++)
  {
    fullInventory.SetSlot(slot, new ItemStack(1, 99));
  }

  fullInventory.SetSlot(0, new ItemStack(5395, 2));
  _ = fullInventoryExtractinatorSimulation.WorldGrid.TrySetTile(
    100,
    1,
    new WorldTile(IsActive: true, Type: ExtractinatorSystem.ExtractinatorTileType));
  fullInventoryExtractinatorSimulation.QueueUseExtractinator(player, 0, 100, 1);
  fullInventoryExtractinatorSimulation.Tick(new SimulationInputBatch());
  if (fullInventory.GetSlot(0) != new ItemStack(5395, 1) ||
      fullInventoryExtractinatorSimulation.CreateWorldItemSnapshots().Count != 1 ||
      fullInventoryExtractinatorSimulation.CreateExtractinatorResultEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A full inventory did not receive Extractinator output as a world item fallback.");
  }
}

Console.WriteLine("PASS: Extractinator direct use is server-owned and atomic");

VerifyInventoryCommandsCommitAtTickBoundary();

static void VerifyInventoryCommandsCommitAtTickBoundary()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(1, 6));

  simulation.QueueTransferItem(player, sourceSlot: 0, destinationSlot: 1, quantity: 2);
  simulation.Tick(new SimulationInputBatch());
  if (inventory.GetSlot(0) != new ItemStack(1, 4) ||
      inventory.GetSlot(1) != new ItemStack(1, 2))
  {
    throw new InvalidOperationException("Inventory transfer did not commit at the tick boundary.");
  }

  simulation.QueueSplitItemStack(player, sourceSlot: 0, destinationSlot: 2, quantity: 1);
  simulation.Tick(new SimulationInputBatch());
  if (inventory.GetSlot(0) != new ItemStack(1, 3) ||
      inventory.GetSlot(2) != new ItemStack(1, 1))
  {
    throw new InvalidOperationException("Inventory split did not commit at the tick boundary.");
  }

  simulation.QueueMergeItemStack(player, sourceSlot: 2, destinationSlot: 1, quantity: 1);
  simulation.Tick(new SimulationInputBatch());
  if (inventory.GetSlot(1) != new ItemStack(1, 3) || !inventory.GetSlot(2).IsEmpty)
  {
    throw new InvalidOperationException("Inventory merge did not commit at the tick boundary.");
  }

  simulation.QueueDropItem(player, sourceSlot: 1, quantity: 2);
  simulation.Tick(new SimulationInputBatch());
  if (inventory.GetSlot(1) != new ItemStack(1, 1) ||
      simulation.CreateWorldItemSnapshots().Single().Stack != new ItemStack(1, 2))
  {
    throw new InvalidOperationException("Inventory drop did not create one server-owned world item.");
  }
}

static PlayerPersistentState CreatePersistentState(
  string uuid,
  int firstItemType,
  int firstItemStack,
  int retainedItemType,
  int retainedItemStack)
{
  PlayerPersistentItem[] items = new PlayerPersistentItem[PlayerPersistentState.ItemSlotCount];
  for (int slotId = 0; slotId < items.Length; slotId++)
  {
    items[slotId] = new PlayerPersistentItem(slotId, 0, 0, 0, false, false);
  }

  items[0] = new PlayerPersistentItem(0, firstItemStack, 11, firstItemType, true, true);
  items[10] = new PlayerPersistentItem(10, retainedItemStack, 0, retainedItemType, false, false);
  return new PlayerPersistentState(
    uuid,
    new PlayerPersistentProfile("Imported"),
    120,
    200,
    80,
    200,
    [new PlayerPersistentBuff(190)],
    selectedLoadout: 1,
    accessoryVisibility: 3,
    items);
}
