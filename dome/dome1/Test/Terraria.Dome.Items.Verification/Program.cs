using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Inventory.Systems;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Compatibility;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Items.Snapshots;
using Terraria.Dome.Simulation.Items.Systems;
using Terraria.Dome.Simulation.Loot;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Definitions;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation.StatusEffects.Definitions;
using Terraria.Dome.Simulation.WorldModel;

ItemDefinitionRegistry definitions = new([new ItemDefinition(1, 99)]);
InventoryComponent inventory = new();
InventoryTransferSystem transfers = new();
int remainder = transfers.TransferIntoSlot(inventory, 0, new ItemStack(1, 70), definitions);
if (remainder != 0 || inventory.GetSlot(0) != new ItemStack(1, 70))
{
  throw new InvalidOperationException("Inventory did not accept an authoritative initial stack.");
}

InventoryComponent atomicStateInventory = new();
long atomicStateRevision = atomicStateInventory.Revision;
try
{
  _ = transfers.TransferIntoSlot(
    atomicStateInventory,
    0,
    new ItemStack(1, 1),
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      NameOverride: new string('x', 201)),
    definitions);
  throw new InvalidOperationException(
    "Inventory transfer accepted an invalid instance state.");
}
catch (ArgumentOutOfRangeException)
{
}

if (!atomicStateInventory.GetSlot(0).IsEmpty ||
    atomicStateInventory.Revision != atomicStateRevision)
{
  throw new InvalidOperationException(
    "Inventory transfer mutated state before rejecting invalid instance metadata.");
}

InventoryComponent sanitizationInventory = new();
sanitizationInventory.SetSlot(0, new ItemStack(1, 100));
new ItemInventorySanitizationSystem().Sanitize(sanitizationInventory, definitions);
if (!sanitizationInventory.GetSlot(0).IsEmpty)
{
  throw new InvalidOperationException(
    "Inventory sanitization silently retained an over-limit stack.");
}

InventoryComponent oversizedNameInventory = new();
oversizedNameInventory.SetSlot(0, new ItemStack(1, 1));
try
{
  oversizedNameInventory.SetInstanceState(
    0,
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      NameOverride: new string('x', 201)));
  throw new InvalidOperationException(
    "Inventory accepted an item name override beyond the persistence limit.");
}
catch (ArgumentOutOfRangeException)
{
}

if (!ItemStack.Empty.IsAir || !new ItemStack(1, 0).IsAir || new ItemStack(1, 1).IsAir)
{
  throw new InvalidOperationException("Item stack air classification did not follow the legacy type rule.");
}

if (!new ItemStack(1, 0).IsActive || ItemStack.Empty.IsActive)
{
  throw new InvalidOperationException("Item stack active classification did not follow the legacy type rule.");
}

if (!ItemCurrencySystem.IsCoinType(71) || !ItemCurrencySystem.IsCoinType(74) ||
    ItemCurrencySystem.IsCoinType(70) || ItemCurrencySystem.IsCoinType(75))
{
  throw new InvalidOperationException("Coin classification did not preserve legacy coin type bounds.");
}

if (!new ItemDefinition(71, 100).IsACoin || new ItemDefinition(70, 100).IsACoin)
{
  throw new InvalidOperationException("Item definition coin classification was not authoritative.");
}

ItemDefinitionRegistry mixedUseDefinitions = new([
  new ItemDefinition(2, 10, HealthRestore: 5, Use: new ItemUseDefinition())]);
InventoryComponent mixedUseInventory = new();
mixedUseInventory.SetSlot(0, new ItemStack(2, 1));
Terraria.Dome.Simulation.Player.Systems.PlayerItemUseSystem mixedUseSystem = new();
Terraria.Dome.Simulation.Inventory.Components.ItemUseStateComponent mixedUseState = new();
HealthComponent mixedUseHealth = new(10, 20);
if (!mixedUseSystem.TryUse(
      mixedUseInventory,
      ref mixedUseState,
      ref mixedUseHealth,
      0,
      mixedUseDefinitions) || mixedUseHealth.Current != 15)
{
  throw new InvalidOperationException(
    "Player item use masked legacy health recovery when Use metadata was present.");
}

Terraria.Dome.Simulation.Inventory.Components.ItemUseStateComponent playerForgedUseState = new()
{
  UseRevision = -1
};
mixedUseInventory.SetSlot(0, new ItemStack(2, 1));
HealthComponent forgedUseHealth = new(10, 20);
if (mixedUseSystem.TryUse(
      mixedUseInventory,
      ref playerForgedUseState,
      ref forgedUseHealth,
      0,
      mixedUseDefinitions))
{
  throw new InvalidOperationException(
    "Player item use accepted a negative use revision.");
}

Terraria.Dome.Simulation.Inventory.Components.ItemUseStateComponent invalidHealthUseState = new();
HealthComponent invalidHealth = new(-1, 20);
if (mixedUseSystem.TryUse(
      mixedUseInventory,
      ref invalidHealthUseState,
      ref invalidHealth,
      0,
      mixedUseDefinitions))
{
  throw new InvalidOperationException(
    "Player item use accepted a health state outside its valid range.");
}

ItemDefinition nonConsumableUseDefinition = new(
  3,
  10,
  Use: new ItemUseDefinition(HealthRestore: 5, Consumable: false));
ItemDefinitionRegistry nonConsumableDefinitions = new([nonConsumableUseDefinition]);
InventoryComponent nonConsumableInventory = new();
nonConsumableInventory.SetSlot(0, new ItemStack(3, 2));
Terraria.Dome.Simulation.Inventory.Components.ItemUseStateComponent nonConsumableState = new();
HealthComponent nonConsumableHealth = new(10, 20);
if (!mixedUseSystem.TryUse(
      nonConsumableInventory,
      ref nonConsumableState,
      ref nonConsumableHealth,
      0,
      nonConsumableDefinitions) || nonConsumableInventory.GetSlot(0) != new ItemStack(3, 2))
{
  throw new InvalidOperationException(
    "Player item use consumed a recovery item that was not marked consumable.");
}

remainder = transfers.TransferIntoSlot(inventory, 0, new ItemStack(1, 50), definitions);
if (remainder != 21 || inventory.GetSlot(0) != new ItemStack(1, 99))
{
  throw new InvalidOperationException("Inventory did not enforce authoritative stack limits.");
}

InventoryComponent invalidTargetInventory = new();
invalidTargetInventory.SetSlot(0, new ItemStack(1, 100));
int invalidTargetRemainder = transfers.TransferIntoSlot(
  invalidTargetInventory,
  0,
  new ItemStack(1, 1),
  definitions);
if (invalidTargetRemainder != 1 || invalidTargetInventory.GetSlot(0) != new ItemStack(1, 100))
{
  throw new InvalidOperationException(
    "Inventory transfer mutated an already over-limit target stack.");
}

ItemDefinitionRegistry uniqueDefinitions = new([
  new ItemDefinition(5, 99, Identity: new ItemIdentityDefinition(UniqueStack: true))]);
InventoryComponent uniqueInventory = new();
int uniqueRemainder = transfers.TransferIntoSlot(
  uniqueInventory,
  0,
  new ItemStack(5, 2),
  uniqueDefinitions);
if (uniqueRemainder != 2 || !uniqueInventory.GetSlot(0).IsEmpty)
{
  throw new InvalidOperationException(
    "Inventory transfer accepted multiple instances of a unique-stack item.");
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
  using Arch.Core.World world = Arch.Core.World.Create();
  WorldItemStore store = new(world);
  store.Add(new WorldItemComponent(
    2,
    new ItemStack(1, 1),
    new SimulationVector(0.0f, 0.0f),
    false,
    1,
    new WorldSectionCoordinates(0, 0),
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(0)));
  throw new InvalidOperationException(
    "World item store accepted an active/empty state mismatch.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new EquipmentSnapshot(
    new PlayerHandle(1),
    [new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
      ItemEquipmentSlot.Head,
      -1,
      false)],
    1);
  throw new InvalidOperationException("Equipment snapshot accepted an invalid source slot.");
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

try
{
  ItemDefinitionCompiler.Validate(new ItemDefinition(
    6,
    1,
    Use: new ItemUseDefinition(ShootType: 1, ShootSpeed: float.NaN)));
  throw new InvalidOperationException("Item compiler accepted a non-finite use projectile speed.");
}
catch (ArgumentException)
{
}

try
{
  ItemDefinitionCompiler.Validate(new ItemDefinition(
    7,
    1,
    Combat: new ItemCombatDefinition(
      Damage: 1,
      DamageClass: ItemDamageClass.Melee,
      ProjectileType: 1,
      ProjectileSpeed: float.PositiveInfinity)));
  throw new InvalidOperationException("Item compiler accepted a non-finite combat projectile speed.");
}
catch (ArgumentException)
{
}

Console.WriteLine("PASS: item definition compiler rejects non-finite projectile speeds");

LootTable lootTable = new(new WorldSeed(1234));
ItemStack maximumQuantityDrop = lootTable.Roll(
  lootTableId: 1,
  replicationId: 1,
  itemType: 1,
  minimumQuantity: 1,
  maximumQuantity: int.MaxValue);
if (maximumQuantityDrop.Quantity < 1 || maximumQuantityDrop.Quantity > int.MaxValue)
{
  throw new InvalidOperationException(
    "Loot quantity rolling did not preserve the full int quantity range.");
}

Console.WriteLine("PASS: loot quantity rolling handles int.MaxValue upper bounds");

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

ItemDefinition nonStackingDefinition = new(19, 1);
ItemDefinitionRegistry nonStackingRegistry = new([nonStackingDefinition]);
InventoryComponent nonStackingInventory = new();
nonStackingInventory.SetSlot(0, new ItemStack(19, 1));
if (new InventoryTransferSystem().TransferIntoSlot(
      nonStackingInventory,
      0,
      new ItemStack(19, 1),
      nonStackingRegistry) != 1 ||
    nonStackingDefinition.CanStack)
{
  throw new InvalidOperationException("Single-slot item incorrectly entered a stack merge path.");
}

InventoryComponent forgedSplitInventory = new();
forgedSplitInventory.SetSlot(0, new ItemStack(19, 2));
InventoryCommandSystem inventoryCommands = new();
if (inventoryCommands.TrySplit(
      forgedSplitInventory,
      0,
      1,
      1,
      nonStackingRegistry,
      out _) || !forgedSplitInventory.GetSlot(1).IsEmpty)
{
  throw new InvalidOperationException("Single-slot item bypassed split stack validation.");
}

InventoryComponent invalidStateSplitInventory = new();
invalidStateSplitInventory.SetSlot(0, new ItemStack(1, 2));
invalidStateSplitInventory.SetInstanceState(
  0,
  new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent());
long invalidStateSplitRevision = invalidStateSplitInventory.Revision;
// Simulate a forged component bypassing InventoryComponent.SetInstanceState validation.
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent[] invalidStateSplitStates =
  (Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent[])typeof(InventoryComponent)
    .GetField("_instanceStates", System.Reflection.BindingFlags.Instance |
      System.Reflection.BindingFlags.NonPublic)!
    .GetValue(invalidStateSplitInventory)!;
invalidStateSplitStates[0] = new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
  NameOverride: new string('x', 201));
if (inventoryCommands.TrySplit(
      invalidStateSplitInventory,
      0,
      1,
      1,
      definitions,
      out _) ||
    invalidStateSplitInventory.GetSlot(0) != new ItemStack(1, 2) ||
    invalidStateSplitInventory.Revision != invalidStateSplitRevision)
{
  throw new InvalidOperationException(
    "Inventory split mutated the source before rejecting invalid instance metadata.");
}

InventoryComponent singleItemTransferInventory = new();
singleItemTransferInventory.SetSlot(0, new ItemStack(19, 1));
if (!inventoryCommands.TryTransfer(
      singleItemTransferInventory,
      0,
      1,
      1,
      nonStackingRegistry,
      out _) || singleItemTransferInventory.GetSlot(1) != new ItemStack(19, 1))
{
  throw new InvalidOperationException("A single non-stackable item could not move to an empty slot.");
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
if (typeof(WorldItemSpawnSystem).GetConstructor([typeof(ItemDefinitionRegistry)]) is null)
{
  throw new InvalidOperationException(
    "World item spawn does not expose an authoritative ItemDefinition owner.");
}

try
{
  _ = new WorldItemSpawnSystem(null!);
  throw new InvalidOperationException(
    "World item spawn accepted a null authoritative ItemDefinition owner.");
}
catch (ArgumentNullException)
{
}

ItemDefinitionRegistry worldItemDefinitions = new([new ItemDefinition(1, 5)]);
WorldItemSpawnSystem authoritativeWorldItemSpawn = new(worldItemDefinitions);
int authoritativeWorldItemId = 1;
if (!authoritativeWorldItemSpawn.TryCreate(
      ref authoritativeWorldItemId,
      new CreateWorldItemCommand(
        new ItemStack(1, 5),
        new SimulationVector(0.0f, 0.0f),
        default,
        SpawnSource: 1),
      out WorldItemComponent authoritativeWorldItem,
      out _) ||
    authoritativeWorldItem.Stack != new ItemStack(1, 5) ||
    authoritativeWorldItemId != 2)
{
  throw new InvalidOperationException(
    "Authoritative world item spawn rejected a valid Definition-bounded stack.");
}

if (authoritativeWorldItemSpawn.TryCreate(
      ref authoritativeWorldItemId,
      new CreateWorldItemCommand(
        new ItemStack(99, 1),
        new SimulationVector(0.0f, 0.0f),
        default,
        SpawnSource: 1),
      out _,
      out _) ||
    authoritativeWorldItemId != 2)
{
  throw new InvalidOperationException(
    "Authoritative world item spawn accepted an unknown item type.");
}

if (authoritativeWorldItemSpawn.TryCreate(
      ref authoritativeWorldItemId,
      new CreateWorldItemCommand(
        new ItemStack(1, 6),
        new SimulationVector(0.0f, 0.0f),
        default,
        SpawnSource: 1),
      out _,
      out _) ||
    authoritativeWorldItemId != 2)
{
  throw new InvalidOperationException(
    "Authoritative world item spawn accepted a stack beyond its Definition limit.");
}

int nextWorldItemId = 1;
using (DomeSimulation stackLimitSimulation = new(new WorldGrid(400, 300)))
{
  try
  {
    stackLimitSimulation.SpawnWorldItem(
      new ItemStack(1, 100),
      new SimulationVector(0.0f, 0.0f));
    throw new InvalidOperationException("World item spawn accepted a stack beyond its Definition limit.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}
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

int maxWorldItemId = int.MaxValue;
if (worldItemSpawn.TryCreate(
      ref maxWorldItemId,
      new CreateWorldItemCommand(
        new ItemStack(1, 1),
        new SimulationVector(0.0f, 0.0f),
        default,
        SpawnSource: 1),
      out _,
      out _) ||
    maxWorldItemId != int.MaxValue)
{
  throw new InvalidOperationException(
    "World item ECS spawn accepted an allocator value that cannot advance.");
}

Console.WriteLine("PASS: world item spawn rejects max replication allocator before overflow");

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

WorldItemComponent delayOverflowWorldItem = delayedWorldItem with
{
  Revision = long.MaxValue,
  WorldState = delayedWorldItem.WorldState with { Revision = long.MaxValue }
};
if (worldItemPickupDelay.TryAdvance(delayOverflowWorldItem, out _))
{
  throw new InvalidOperationException(
    "World item pickup delay accepted a revision that cannot advance.");
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

WorldItemComponent motionOverflowWorldItem = movedWorldItem with
{
  Revision = long.MaxValue,
  WorldState = movedWorldItem.WorldState with { Revision = long.MaxValue }
};
if (worldItemMotion.TryMove(
    motionOverflowWorldItem,
    new MoveWorldItemCommand(
      motionOverflowWorldItem.ReplicationId,
      new SimulationVector(2.0f, 0.0f),
      long.MaxValue),
    default,
    out _))
{
  throw new InvalidOperationException(
    "World item motion accepted a revision that cannot advance.");
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

WorldItemComponent overflowWorldItem = movedWorldItem with
{
  Revision = long.MaxValue,
  WorldState = movedWorldItem.WorldState with { Revision = long.MaxValue }
};
if (worldItemDestroy.TryDestroy(
    overflowWorldItem,
    new DestroyWorldItemCommand(overflowWorldItem.ReplicationId, long.MaxValue),
    out _,
    out _))
{
  throw new InvalidOperationException(
    "World item destruction accepted a revision that cannot advance.");
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

WorldItemComponent ownerRevisionOverflowWorldItem = movedWorldItem with
{
  IsActive = true,
  Stack = new ItemStack(1, 1),
  Revision = 1,
  WorldState = movedWorldItem.WorldState with
  {
    IsActive = true,
    LastOwnerRevision = long.MaxValue,
    Revision = 1,
    ReservedPlayerId = Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.UnreservedPlayerId
  }
};
InventoryComponent ownerRevisionOverflowInventory = new();
if (worldItemPickup.TryPickup(
      ref ownerRevisionOverflowWorldItem,
      new PickupWorldItemCommand(new PlayerHandle(1), ownerRevisionOverflowWorldItem.ReplicationId),
      new SimulationVector(1.0f, 0.0f),
      ownerRevisionOverflowInventory,
      definitions,
      pickupRange: 3.0f,
      out _,
      out _) ||
    ownerRevisionOverflowWorldItem.WorldState.LastOwnerRevision != long.MaxValue ||
    !ownerRevisionOverflowInventory.GetSlot(0).IsEmpty)
{
  throw new InvalidOperationException(
    "World item pickup accepted an owner revision that cannot advance.");
}

Console.WriteLine("PASS: world item pickup rejects max owner revision before overflow");

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

WorldItemComponent invalidStateMergeReceiver = finiteDonor with
{
  ReplicationId = 4,
  InstanceState = new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
    NameOverride: new string('x', 201))
};
if (worldItemStacking.TryMerge(
      invalidStateMergeReceiver,
      new WorldItemComponent(
        5,
        new ItemStack(1, 1),
        new SimulationVector(0.0f, 0.0f),
        true,
        1,
        default,
        Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(5)),
      definitions,
      tick: 8,
      maximumDistance: 1.0f,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "World item stacking propagated invalid instance state.");
}

WorldItemComponent mergeOverflowReceiver = finiteDonor with
{
  Revision = long.MaxValue,
  WorldState = finiteDonor.WorldState with { Revision = long.MaxValue }
};
WorldItemComponent mergeOverflowDonor = new(
  3,
  new ItemStack(1, 1),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(3));
if (worldItemStacking.TryMerge(
      mergeOverflowReceiver,
      mergeOverflowDonor,
      definitions,
      tick: 9,
      maximumDistance: 1.0f,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "World item stacking accepted a revision that cannot advance.");
}

Console.WriteLine("PASS: compatible world items passively merge under stack limits");

WorldItemComponent overLimitDonor = new(
  3,
  new ItemStack(1, 100),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(3));
WorldItemComponent validReceiver = new(
  4,
  new ItemStack(1, 1),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  default,
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(4));
if (worldItemStacking.TryMerge(
      validReceiver,
      overLimitDonor,
      new ItemDefinitionRegistry([new ItemDefinition(1, 99)]),
      tick: 1,
      maximumDistance: 10.0f,
      out _,
      out _))
{
  throw new InvalidOperationException("World stacking accepted an over-limit donor stack.");
}

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

WorldItemComponent forgedPickupItem = new(
  99,
  new ItemStack(1, 100),
  new SimulationVector(0.0f, 0.0f),
  true,
  0,
  default);
InventoryComponent forgedPickupInventory = new();
if (new WorldItemPickupSystem().TryPickup(
      ref forgedPickupItem,
      new PickupWorldItemCommand(new PlayerHandle(1), 99),
      new SimulationVector(0.0f, 0.0f),
      forgedPickupInventory,
      new ItemDefinitionRegistry([new ItemDefinition(1, 99)]),
      10.0f,
      out _,
      out _) ||
    forgedPickupItem.Stack != new ItemStack(1, 100) ||
    !forgedPickupInventory.GetSlot(0).IsEmpty)
{
  throw new InvalidOperationException("World pickup bypassed the item definition stack limit.");
}

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

InventoryComponent forgedInputInventory = new();
forgedInputInventory.SetSlot(0, new ItemStack(1, 100));
ItemInputValidationResult forgedInput = inputValidation.Validate(
  true,
  forgedInputInventory,
  0,
  new ItemDefinitionRegistry([new ItemDefinition(1, 99)]));
if (forgedInput.IsAccepted)
{
  throw new InvalidOperationException("Selected item input bypassed the definition stack limit.");
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

if (!itemUseTransactionState.IsUsing || !itemUseTransactionState.JustStarted ||
    itemUseTransactionState.UseRevision != 1)
{
  throw new InvalidOperationException(
    "Direct item-use transaction did not publish its runtime lifecycle state.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent directRevisionOverflowState = new()
{
  AnimationTicks = 2,
  IsUsing = true,
  IsChanneling = true,
  UseRevision = int.MaxValue
};
ItemUseResult directRevisionOverflow = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25, UseCooldownTicks: 4),
  ref directRevisionOverflowState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 2);
if (directRevisionOverflow.IsAccepted || directRevisionOverflow.Health != 50 ||
    directRevisionOverflow.Mana != 0 || directRevisionOverflowState.AnimationTicks != 2 ||
    !directRevisionOverflowState.IsUsing || !directRevisionOverflowState.IsChanneling ||
    directRevisionOverflowState.UseRevision != int.MaxValue)
{
  throw new InvalidOperationException(
    "Direct item-use accepted a revision that cannot advance without mutation.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent invalidPlayerUseState = new();
ItemUseResult invalidPlayerUse = new ItemUseSystem().TryUse(
  new PlayerHandle(0),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25),
  ref invalidPlayerUseState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 1);
if (invalidPlayerUse.IsAccepted)
{
  throw new InvalidOperationException("Item use transaction accepted an invalid player handle.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent invalidSlotUseState = new();
ItemUseResult invalidSlotUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  InventoryComponent.SlotCount,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25),
  ref invalidSlotUseState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 1);
if (invalidSlotUse.IsAccepted || invalidSlotUseState.CooldownTicks != 0)
{
  throw new InvalidOperationException("Item use transaction accepted an invalid inventory slot.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent metadataUseState = new();
ItemUseResult metadataUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, Use: new ItemUseDefinition(UseAnimation: 2)),
  ref metadataUseState,
  health: 0,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 1);
if (!metadataUse.IsAccepted)
{
  throw new InvalidOperationException("Item use metadata did not create an executable action.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent mismatchedUseState = new();
ItemUseResult mismatchedUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(2, 1),
  new ItemDefinition(1, 99, HealthRestore: 25),
  ref mismatchedUseState,
  health: 0,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 2);
if (mismatchedUse.IsAccepted || mismatchedUseState.CooldownTicks != 0)
{
  throw new InvalidOperationException("Item use accepted a stack mismatched with its definition.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent invalidUseState = new()
{
  CooldownTicks = -1
};
ItemUseResult invalidStateUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25),
  ref invalidUseState,
  health: 0,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 3);
if (invalidStateUse.IsAccepted)
{
  throw new InvalidOperationException("Item use accepted a negative runtime timer.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent combinedMetadataUseState = new();
ItemUseResult combinedMetadataUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(
    1,
    99,
    HealthRestore: 25,
    UseCooldownTicks: 7,
    Use: new ItemUseDefinition(UseAnimation: 2)),
  ref combinedMetadataUseState,
  health: 0,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: 2);
if (!combinedMetadataUse.IsAccepted || combinedMetadataUse.Health != 25 ||
    combinedMetadataUseState.CooldownTicks != 7)
{
  throw new InvalidOperationException(
    "Legacy recovery and cooldown defaults were dropped when use metadata was present.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent overflowRecoveryState = new();
ItemUseResult overflowRecovery = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: int.MaxValue),
  ref overflowRecoveryState,
  health: int.MaxValue - 1,
  maximumHealth: int.MaxValue,
  mana: 0,
  maximumMana: int.MaxValue,
  sequence: 1);
if (!overflowRecovery.IsAccepted || overflowRecovery.Health != int.MaxValue)
{
  throw new InvalidOperationException("Item health recovery overflowed instead of saturating.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent overflowManaState = new();
ItemUseResult overflowMana = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, Use: new ItemUseDefinition(ManaRestore: int.MaxValue)),
  ref overflowManaState,
  health: 0,
  maximumHealth: int.MaxValue,
  mana: int.MaxValue - 1,
  maximumMana: int.MaxValue,
  sequence: 1);
if (!overflowMana.IsAccepted || overflowMana.Mana != int.MaxValue)
{
  throw new InvalidOperationException("Item mana recovery overflowed instead of saturating.");
}

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent invalidSequenceState = new();
ItemUseResult invalidSequenceUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(1, 1),
  new ItemDefinition(1, 99, HealthRestore: 25),
  ref invalidSequenceState,
  health: 50,
  maximumHealth: 100,
  mana: 0,
  maximumMana: 20,
  sequence: -1);
if (invalidSequenceUse.IsAccepted)
{
  throw new InvalidOperationException("Item use accepted a negative command sequence.");
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
        CooldownTicks: 2,
        ReuseDelayTicks: 5,
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
    channelState.AnimationTicks != 6 || channelState.CooldownTicks != 5 || !channelState.IsChanneling)
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

Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent summonUseState = new();
ItemUseResult summonUse = new ItemUseSystem().TryUse(
  new PlayerHandle(1),
  0,
  new ItemStack(DomeSimulation.FixtureItemNpcSummonType, 1),
  new ItemDefinition(
    DomeSimulation.FixtureItemNpcSummonType,
    20,
    Summoning: new ItemSummoningDefinition(NpcType: 1)),
  ref summonUseState,
  health: 100,
  maximumHealth: 100,
  mana: 20,
  maximumMana: 20,
  sequence: 7);
if (!summonUse.IsAccepted || summonUse.ConsumedQuantity != 0)
{
  throw new InvalidOperationException(
    "An item with only makeNPC metadata was not exposed as an executable summon action.");
}

ItemUseCooldownSystem cooldown = new();
cooldown.Tick(ref itemUseTransactionState);
if (itemUseTransactionState.CooldownTicks != 9 || itemUseTransactionState.JustStarted ||
    !itemUseTransactionState.IsUsing)
{
  throw new InvalidOperationException(
    "Item-use cooldown did not clear the start edge while retaining active use state.");
}

for (int tick = 1; tick < 10; tick++)
{
  cooldown.Tick(ref itemUseTransactionState);
}

if (!itemUseTransactionState.CanUse || itemUseTransactionState.IsUsing)
{
  throw new InvalidOperationException(
    "Item-use cooldown did not end active use state deterministically.");
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

InventoryComponent forgedAmmoInventory = new();
forgedAmmoInventory.SetSlot(0, new ItemStack(3, 2));
ItemDefinitionRegistry singleAmmoDefinition = new([new ItemDefinition(3, 1)]);
if (new ItemAmmoConsumptionSystem().HasAmmo(
      forgedAmmoInventory,
      3,
      singleAmmoDefinition))
{
  throw new InvalidOperationException("Ammunition lookup bypassed the definition stack limit.");
}

InventoryComponent mixedAmmoInventory = new();
mixedAmmoInventory.SetSlot(0, new ItemStack(3, 2));
mixedAmmoInventory.SetSlot(1, new ItemStack(3, 1));
if (!new ItemAmmoConsumptionSystem().TryConsume(
      mixedAmmoInventory,
      3,
      singleAmmoDefinition,
      out int mixedAmmoSlot) ||
    mixedAmmoSlot != 1 || mixedAmmoInventory.GetSlot(0) != new ItemStack(3, 2) ||
    !mixedAmmoInventory.GetSlot(1).IsEmpty)
{
  throw new InvalidOperationException(
    "Ammunition consumption selected an over-limit stack before a valid stack.");
}

Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent equipmentState;
ItemDefinition equipableHeadDefinition = new(
  4,
  1,
  Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 5));
if (!new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    equipableHeadDefinition,
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
    new PlayerHandle(0),
    new ItemStack(4, 1),
    0,
    equipableHeadDefinition,
    existing: null,
    vanity: false,
    out _,
    out _,
    out _))
{
  throw new InvalidOperationException("An item equipment transaction accepted an invalid player handle.");
}

if (new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    InventoryComponent.SlotCount,
    equipableHeadDefinition,
    existing: null,
    vanity: false,
    out _,
    out _,
    out _))
{
  throw new InvalidOperationException("An item equipment transaction accepted an out-of-range source slot.");
}

if (new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    equipableHeadDefinition,
    existing: null,
    vanity: true,
    out _,
    out _,
    out _))
{
  throw new InvalidOperationException("A non-vanity equipment definition accepted a vanity request.");
}

if (new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    equipableHeadDefinition,
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

if (new ItemEquipmentSystem().TryEquip(
    new PlayerHandle(1),
    new ItemStack(4, 1),
    0,
    new ItemDefinition(
      4,
      1,
      Equipment: new ItemEquipmentDefinition((ItemEquipmentSlot)255)),
    existing: null,
    vanity: false,
    out _,
    out _,
    out _))
{
  throw new InvalidOperationException("An undefined equipment slot enum was accepted.");
}

if (new ItemEquipmentSystem().TryEquip(
      new PlayerHandle(1),
      new ItemStack(5, 1),
      0,
      equipableHeadDefinition,
      existing: null,
      vanity: false,
      out _,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "Equipment accepted a stack whose item type did not match its definition.");
}

if (new ItemEquipmentSystem().TryEquip(
      new PlayerHandle(1),
      new ItemStack(4, 2),
      0,
      equipableHeadDefinition,
      existing: null,
      vanity: false,
      out _,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "Equipment accepted a stack above the definition stack limit.");
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

if (new ItemPlacementSystem().TryCreateTileCommand(
    metadataDefinitions.Get(5),
    3,
    4,
    -1,
    out _,
    out _))
{
  throw new InvalidOperationException(
    "Item placement emitted a tile command with a negative sequence.");
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

IReadOnlyList<CreateWorldItemCommand> maximumRangeDrops = dropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(3, 1, int.MaxValue)],
  expertMode: false,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
if (maximumRangeDrops.Count != 1 || maximumRangeDrops[0].Stack.IsEmpty ||
    maximumRangeDrops[0].Stack.Quantity < 1)
{
  throw new InvalidOperationException(
    "Item drop quantity range overflow rejected a valid bounded definition.");
}

if (typeof(ItemDropRuleSystem).GetConstructor([typeof(ItemDefinitionRegistry)]) is null)
{
  throw new InvalidOperationException(
    "Item drop rules do not expose an authoritative ItemDefinition owner.");
}

try
{
  _ = new ItemDropRuleSystem(null!);
  throw new InvalidOperationException(
    "Item drop rules accepted a null authoritative ItemDefinition owner.");
}
catch (ArgumentNullException)
{
}

ItemDefinitionRegistry dropDefinitions = new([
  new ItemDefinition(3, 2),
  new ItemDefinition(4, 1)
]);
ItemDropRuleSystem authoritativeDropRules = new(dropDefinitions);
IReadOnlyList<CreateWorldItemCommand> validAuthoritativeDrops = authoritativeDropRules.Evaluate(
  new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
  sourceEntityId: 7,
  tick: 12,
  [new ItemDropDefinition(3, 1, 2)],
  expertMode: false,
  masterMode: false,
  new SimulationVector(2.0f, 3.0f),
  default,
  spawnSource: 2);
if (validAuthoritativeDrops.Count != 1 || validAuthoritativeDrops[0].Stack.ItemType != 3 ||
    validAuthoritativeDrops[0].Stack.Quantity < 1 ||
    validAuthoritativeDrops[0].Stack.Quantity > 2)
{
  throw new InvalidOperationException(
    "Authoritative item drop rules rejected a valid bounded definition.");
}

if (authoritativeDropRules.Evaluate(
      new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
      sourceEntityId: 7,
      tick: 12,
      [new ItemDropDefinition(99)],
      expertMode: false,
      masterMode: false,
      new SimulationVector(2.0f, 3.0f),
      default,
      spawnSource: 2).Count != 0 ||
    authoritativeDropRules.Evaluate(
      new Terraria.Dome.Simulation.WorldModel.WorldSeed(42),
      sourceEntityId: 7,
      tick: 12,
      [new ItemDropDefinition(3, 1, 3)],
      expertMode: false,
      masterMode: false,
      new SimulationVector(2.0f, 3.0f),
      default,
      spawnSource: 2).Count != 0)
{
  throw new InvalidOperationException(
    "Authoritative item drop rules emitted an unknown or over-limit item.");
}

Console.WriteLine("PASS: item drop quantity range handles Int32 maximum without overflow");

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

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent overLimitPrefixState = new();
if (new ItemPrefixSystem().TryApply(
    new ItemStack(3, 100),
    prefixDefinition,
    ref overLimitPrefixState,
    prefixId: 5,
    out _,
    out _))
{
  throw new InvalidOperationException("Item prefix accepted a stack above the definition limit.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent invalidPrefixState =
  new(NameOverride: new string('x', 201));
if (new ItemPrefixSystem().TryApply(
    new ItemStack(3, 1),
    prefixDefinition,
    ref invalidPrefixState,
    prefixId: 5,
    out _,
    out _) ||
    invalidPrefixState.NameOverride is null)
{
  throw new InvalidOperationException(
    "Item prefix application propagated an invalid instance state.");
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

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent invalidVariantState =
  new(NameOverride: new string('x', 201));
if (new ItemVariantSystem().TryApply(
    new ItemStack(3, 1),
    ref invalidVariantState,
    new ItemVariantDefinition(2, 3, 4),
    out _,
    out _) ||
    invalidVariantState.NameOverride is null)
{
  throw new InvalidOperationException(
    "Item variant application propagated an invalid instance state.");
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

if (typeof(ItemPrefixSystem).GetConstructor([typeof(ItemDefinitionRegistry)]) is null)
{
  throw new InvalidOperationException(
    "Item prefix system does not expose an authoritative ItemDefinition owner.");
}

try
{
  _ = new ItemPrefixSystem(null!);
  throw new InvalidOperationException(
    "Item prefix system accepted a null authoritative ItemDefinition owner.");
}
catch (ArgumentNullException)
{
}

ItemDefinitionRegistry authoritativePrefixDefinitions = new([
  new ItemDefinition(3, 2, Prefixes: new ItemPrefixDefinition([5])),
  new ItemDefinition(4, 2, Prefixes: new ItemPrefixDefinition([6]))]);
ItemPrefixSystem authoritativePrefixSystem = new(authoritativePrefixDefinitions);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent authoritativePrefixState = new();
if (!authoritativePrefixSystem.TryApply(
      new ItemStack(3, 2),
      authoritativePrefixDefinitions.Get(3),
      ref authoritativePrefixState,
      prefixId: 5,
      out _,
      out _) ||
    authoritativePrefixState.PrefixId != 5)
{
  throw new InvalidOperationException(
    "Authoritative item prefix rejected a valid registered Definition range.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent forgedPrefixState = new();
ItemDefinition forgedPrefixDefinition = new(
  3,
  2,
  Prefixes: new ItemPrefixDefinition([6]));
if (authoritativePrefixSystem.TryApply(
      new ItemStack(3, 1),
      forgedPrefixDefinition,
      ref forgedPrefixState,
      prefixId: 6,
      out _,
      out _) ||
    forgedPrefixState != default)
{
  throw new InvalidOperationException(
    "Authoritative item prefix accepted a forged Definition prefix table.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent unknownPrefixState = new();
if (authoritativePrefixSystem.TryApply(
      new ItemStack(99, 1),
      new ItemDefinition(99, 2, Prefixes: new ItemPrefixDefinition([5])),
      ref unknownPrefixState,
      prefixId: 5,
      out _,
      out _) ||
    unknownPrefixState != default)
{
  throw new InvalidOperationException(
    "Authoritative item prefix accepted an unknown item type.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent overLimitPrefixOwnerState = new();
if (authoritativePrefixSystem.TryApply(
      new ItemStack(3, 3),
      authoritativePrefixDefinitions.Get(3),
      ref overLimitPrefixOwnerState,
      prefixId: 5,
      out _,
      out _) ||
    overLimitPrefixOwnerState != default)
{
  throw new InvalidOperationException(
    "Authoritative item prefix accepted a stack beyond its Definition limit.");
}

if (typeof(ItemVariantSystem).GetConstructor([typeof(ItemDefinitionRegistry)]) is null)
{
  throw new InvalidOperationException(
    "Item variant system does not expose an authoritative ItemDefinition owner.");
}

try
{
  _ = new ItemVariantSystem(null!);
  throw new InvalidOperationException(
    "Item variant system accepted a null authoritative ItemDefinition owner.");
}
catch (ArgumentNullException)
{
}

ItemDefinitionRegistry authoritativeVariantDefinitions = new([
  new ItemDefinition(3, 2),
  new ItemDefinition(4, 2),
  new ItemDefinition(5, 1)]);
ItemVariantSystem authoritativeVariantSystem = new(authoritativeVariantDefinitions);
Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent authoritativeVariantState =
  new();
if (!authoritativeVariantSystem.TryApply(
      new ItemStack(3, 2),
      ref authoritativeVariantState,
      new ItemVariantDefinition(10, 3, 4),
      out ItemStack authoritativeVariantStack,
      out _) ||
    authoritativeVariantStack != new ItemStack(4, 2) ||
    authoritativeVariantState.VariantId != 10)
{
  throw new InvalidOperationException(
    "Authoritative item variant rejected a valid source and replacement Definition range.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent unknownVariantState = new();
if (authoritativeVariantSystem.TryApply(
      new ItemStack(99, 1),
      ref unknownVariantState,
      new ItemVariantDefinition(11, 99, 4),
      out _,
      out _) ||
    unknownVariantState != default)
{
  throw new InvalidOperationException(
    "Authoritative item variant accepted an unknown source item type.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent overLimitSourceVariantState =
  new();
if (authoritativeVariantSystem.TryApply(
      new ItemStack(3, 3),
      ref overLimitSourceVariantState,
      new ItemVariantDefinition(12, 3, 4),
      out _,
      out _) ||
    overLimitSourceVariantState != default)
{
  throw new InvalidOperationException(
    "Authoritative item variant accepted a source stack beyond its Definition limit.");
}

Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent
  overLimitReplacementVariantState = new();
if (authoritativeVariantSystem.TryApply(
      new ItemStack(3, 2),
      ref overLimitReplacementVariantState,
      new ItemVariantDefinition(13, 3, 5),
      out _,
      out _) ||
    overLimitReplacementVariantState != default)
{
  throw new InvalidOperationException(
    "Authoritative item variant accepted a replacement stack beyond its Definition limit.");
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

try
{
  _ = new EquipmentSnapshot(
    new PlayerHandle(0),
    [],
    1);
  throw new InvalidOperationException("Equipment snapshot accepted an invalid player handle.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new InventorySnapshot(
    new PlayerHandle(1),
    new List<ItemInstanceSnapshot> { null! },
    0,
    1);
  throw new InvalidOperationException("Inventory snapshot accepted a null item instance.");
}
catch (ArgumentException)
{
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

try
{
  new ItemReplicationSnapshot(
    1,
    new ItemStack(1, 1),
    new SimulationVector(0.0f, 0.0f),
    false,
    1,
    new WorldSectionCoordinates(0, 0)).Validate();
  throw new InvalidOperationException(
    "Item replication snapshot accepted an active/empty mismatch.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new ItemInstanceSnapshot(
    new ItemStack(1, 1),
    new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
      NameOverride: new string('x', 201)));
  throw new InvalidOperationException(
    "Item instance snapshot accepted an oversized name override.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  using Arch.Core.World world = Arch.Core.World.Create();
  WorldItemStore store = new(world);
  store.Add(new WorldItemComponent(
      1,
      new ItemStack(1, 1),
      new SimulationVector(0.0f, 0.0f),
      true,
      1,
      new WorldSectionCoordinates(0, 0),
      InstanceState: new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
        NameOverride: new string('x', 201))));
  throw new InvalidOperationException(
    "World item store accepted an oversized name override.");
}
catch (ArgumentOutOfRangeException)
{
}

if (typeof(WorldItemStore).GetConstructor([
      typeof(Arch.Core.World),
      typeof(ItemDefinitionRegistry)]) is null)
{
  throw new InvalidOperationException(
    "World item store does not expose an authoritative ItemDefinition owner.");
}

try
{
  using Arch.Core.World world = Arch.Core.World.Create();
  _ = new WorldItemStore(world, null!);
  throw new InvalidOperationException(
    "World item store accepted a null authoritative ItemDefinition owner.");
}
catch (ArgumentNullException)
{
}

ItemDefinitionRegistry worldItemStoreDefinitions = new([new ItemDefinition(1, 1)]);
using (Arch.Core.World authoritativeWorld = Arch.Core.World.Create())
{
  WorldItemStore authoritativeStore = new(authoritativeWorld, worldItemStoreDefinitions);
  WorldItemComponent validStoreItem = new(
    1,
    new ItemStack(1, 1),
    new SimulationVector(0.0f, 0.0f),
    true,
    1,
    default,
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(0));
  authoritativeStore.Add(validStoreItem);
  try
  {
    authoritativeStore.Add(validStoreItem with
    {
      ReplicationId = 2,
      Stack = new ItemStack(99, 1)
    });
    throw new InvalidOperationException(
      "Authoritative world item store accepted an unknown item type.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    authoritativeStore[1] = validStoreItem with { Stack = new ItemStack(1, 2) };
    throw new InvalidOperationException(
      "Authoritative world item store synchronized a stack beyond its Definition limit.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  if (authoritativeStore[1].Stack != validStoreItem.Stack || authoritativeStore.Count != 1)
  {
    throw new InvalidOperationException(
      "World item store changed state while rejecting an invalid Definition stack.");
  }
}

try
{
  _ = new ItemInstanceSnapshot(new ItemStack(1, 0), default);
  throw new InvalidOperationException("A non-canonical empty item snapshot was accepted.");
}
catch (ArgumentException)
{
}

try
{
  _ = new Terraria.Dome.Simulation.Items.Snapshots.WorldItemSnapshot(
    1,
    new ItemInstanceSnapshot(new ItemStack(1, 1), default),
    new SimulationVector(0.0f, 0.0f),
    default,
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.FromReplicationSnapshot(
      isActive: false,
      revision: 1));
  throw new InvalidOperationException("An inactive world snapshot carried a live item instance.");
}
catch (ArgumentException)
{
}

try
{
  _ = new ItemUseSnapshot(
    new PlayerHandle(1),
    new Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent
    {
      CooldownTicks = -1
    },
    1);
  throw new InvalidOperationException("An item-use snapshot accepted a negative runtime timer.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new Terraria.Dome.Simulation.Items.Snapshots.WorldItemSnapshot(
    1,
    new ItemInstanceSnapshot(new ItemStack(1, 1), default),
    new SimulationVector(float.NaN, 0.0f),
    default,
    Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(1));
  throw new InvalidOperationException("A world snapshot accepted a non-finite position.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new Terraria.Dome.Simulation.Items.Snapshots.WorldItemSnapshot(
    1,
    new ItemInstanceSnapshot(new ItemStack(1, 1), default),
    new SimulationVector(0.0f, 0.0f),
    default,
    new Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent(
      true,
      0,
      1,
      -1,
      -1,
      1));
  throw new InvalidOperationException("A world snapshot accepted a negative owner revision.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = new ItemUseSnapshot(
    new PlayerHandle(0),
    default,
    0);
  throw new InvalidOperationException("An item-use snapshot accepted an invalid player handle.");
}
catch (ArgumentOutOfRangeException)
{
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

if (!useState.JustStarted)
{
  throw new InvalidOperationException("Item-use state did not expose its start edge.");
}

itemUseSystem.Tick(ref useState);
if (useState.JustStarted)
{
  throw new InvalidOperationException("Item-use start edge was not cleared after its tick.");
}

Console.WriteLine("PASS: selection and item use have separate typed runtime state");

InventoryComponent nestedUseInventory = new();
nestedUseInventory.SetSlot(0, new ItemStack(11, 1));
ItemUseStateComponent nestedUseState = new();
HealthComponent nestedUseHealth = new(10, 100);
ItemDefinitionRegistry nestedUseDefinitions = new([
  new ItemDefinition(
    11,
    99,
    Use: new ItemUseDefinition(
      HealthRestore: 15,
      CooldownTicks: 3,
      ReuseDelayTicks: 5))]);
if (!itemUseSystem.TryUse(
      nestedUseInventory,
      ref nestedUseState,
      ref nestedUseHealth,
      0,
      nestedUseDefinitions) ||
    nestedUseHealth.Current != 25 || nestedUseState.CooldownTicks != 5)
{
  throw new InvalidOperationException(
    "The compatibility item-use entry point dropped nested use recovery or cooldown metadata.");
}

InventoryComponent forgedCompatibilityInventory = new();
forgedCompatibilityInventory.SetSlot(0, new ItemStack(11, 100));
ItemUseStateComponent forgedCompatibilityState = new();
HealthComponent forgedCompatibilityHealth = new(10, 100);
if (itemUseSystem.TryUse(
      forgedCompatibilityInventory,
      ref forgedCompatibilityState,
      ref forgedCompatibilityHealth,
      0,
      nestedUseDefinitions))
{
  throw new InvalidOperationException(
    "The compatibility item-use entry point bypassed the definition stack limit.");
}

InventoryComponent overflowPlayerInventory = new();
overflowPlayerInventory.SetSlot(0, new ItemStack(1, 1));
ItemUseStateComponent overflowPlayerUseState = new();
HealthComponent overflowPlayerHealth = new(int.MaxValue - 1, int.MaxValue);
if (!itemUseSystem.TryUse(
      overflowPlayerInventory,
      ref overflowPlayerUseState,
      ref overflowPlayerHealth,
      0,
      consumableDefinitions) || overflowPlayerHealth.Current != int.MaxValue)
{
  throw new InvalidOperationException("Player item-use recovery overflowed instead of saturating.");
}

ItemUseStateComponent useRevisionOverflowState = new() { UseRevision = int.MaxValue };
HealthComponent useRevisionOverflowHealth = new(50, 100);
ItemStack useRevisionOverflowItem = inventory.GetSlot(0);
if (itemUseSystem.TryUse(
      inventory,
      ref useRevisionOverflowState,
      ref useRevisionOverflowHealth,
      0,
      consumableDefinitions) ||
    useRevisionOverflowState.UseRevision != int.MaxValue ||
    useRevisionOverflowHealth.Current != 50 ||
    inventory.GetSlot(0) != useRevisionOverflowItem)
{
  throw new InvalidOperationException(
    "Item use accepted a revision that cannot advance.");
}

Console.WriteLine("PASS: item use rejects max revision before overflow");

SelectedItemComponent selectionRevisionOverflow = new() { Revision = int.MaxValue };
int selectionRevisionOverflowSlot = inventory.SelectedSlot;
new ItemSelectionSystem().Apply(
  inventory,
  ref selectionRevisionOverflow,
  0);
if (selectionRevisionOverflow.Revision != int.MaxValue ||
    inventory.SelectedSlot != selectionRevisionOverflowSlot)
{
  throw new InvalidOperationException(
    "Item selection accepted a revision that cannot advance.");
}

Console.WriteLine("PASS: item selection rejects max revision before overflow");

SelectedItemComponent negativeSelectionRevision = new() { Revision = -1 };
int negativeSelectionSlot = inventory.SelectedSlot;
new ItemSelectionSystem().Apply(inventory, ref negativeSelectionRevision, 0);
if (negativeSelectionRevision.Revision != -1 || inventory.SelectedSlot != negativeSelectionSlot)
{
  throw new InvalidOperationException("Item selection accepted a negative revision.");
}

EquipmentLoadoutComponent loadoutRevisionOverflow = new()
{
  SelectedLoadout = 0,
  AccessoryVisibility = 0,
  Revision = int.MaxValue
};
new EquipmentStatSystem().Apply(ref loadoutRevisionOverflow, selectedLoadout: 1, accessoryVisibility: 1);
if (loadoutRevisionOverflow.SelectedLoadout != 0 ||
    loadoutRevisionOverflow.AccessoryVisibility != 0 ||
    loadoutRevisionOverflow.Revision != int.MaxValue)
{
  throw new InvalidOperationException(
    "Equipment loadout accepted a revision that cannot advance.");
}

Console.WriteLine("PASS: equipment loadout rejects max revision before overflow");

try
{
  new EquipmentStateCollectionComponent().Add(
    new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
      (ItemEquipmentSlot)255,
      0,
      false));
  throw new InvalidOperationException("Equipment state accepted an undefined slot enum.");
}
catch (ArgumentException)
{
}

EquipmentLoadoutComponent negativeLoadoutRevision = new()
{
  Revision = -1
};
new EquipmentStatSystem().Apply(
  ref negativeLoadoutRevision,
  selectedLoadout: 1,
  accessoryVisibility: 1);
if (negativeLoadoutRevision.Revision != -1 ||
    negativeLoadoutRevision.SelectedLoadout != 0 ||
    negativeLoadoutRevision.AccessoryVisibility != 0)
{
  throw new InvalidOperationException(
    "Equipment loadout accepted a negative revision.");
}

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

using (DomeSimulation heldInputSimulation = new(new WorldGrid(400, 300)))
{
  PlayerHandle heldInputPlayer = heldInputSimulation.CreatePlayer(
    new SimulationVector(15.0f, 0.0f));
  InventoryComponent heldInputInventory = heldInputSimulation.GetInventory(heldInputPlayer);
  heldInputInventory.SetSlot(0, new ItemStack(1, 2));
  heldInputSimulation.QueuePlayerDamage(heldInputPlayer, 50);
  heldInputSimulation.Tick(new SimulationInputBatch());

  SimulationInputBatch heldUseInput = new(new PlayerInput(
    heldInputPlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true));
  heldInputSimulation.Tick(heldUseInput);
  if (!heldInputSimulation.TryGetPlayerInputEdges(
        heldInputPlayer,
        out Terraria.Dome.Simulation.Player.PlayerInputEdges initialHeldPress) ||
      !initialHeldPress.WasUseItemPressed ||
      heldInputInventory.GetSlot(0) != new ItemStack(1, 1) ||
      heldInputSimulation.CreateItemUseSnapshot(heldInputPlayer).State.UseRevision != 1)
  {
    throw new InvalidOperationException(
      "The initial held-input item use did not commit exactly once.");
  }

  heldInputSimulation.Tick(heldUseInput);
  if (!heldInputSimulation.TryGetPlayerInputEdges(
        heldInputPlayer,
        out Terraria.Dome.Simulation.Player.PlayerInputEdges heldPress) ||
      heldPress.WasUseItemPressed)
  {
    throw new InvalidOperationException(
      "A held non-auto-reuse input incorrectly produced another press edge.");
  }

  for (int tick = 1; tick < 10; tick++)
  {
    heldInputSimulation.Tick(heldUseInput);
  }

  Terraria.Dome.Simulation.Items.Snapshots.ItemUseSnapshot heldInputSnapshot =
    heldInputSimulation.CreateItemUseSnapshot(heldInputPlayer);
  if (heldInputInventory.GetSlot(0) != new ItemStack(1, 1) ||
      heldInputSnapshot.State.UseRevision != 1)
  {
    throw new InvalidOperationException(
      "A non-auto-reuse item was consumed again while the input remained held.");
  }

  heldInputSimulation.QueuePlayerDamage(heldInputPlayer, 25);
  heldInputSimulation.Tick(new SimulationInputBatch(new PlayerInput(
    heldInputPlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: false)));
  if (!heldInputSimulation.TryGetPlayerInputEdges(
        heldInputPlayer,
        out Terraria.Dome.Simulation.Player.PlayerInputEdges released) ||
      !released.WasUseItemReleased)
  {
    throw new InvalidOperationException(
      "Releasing a held non-auto-reuse input did not publish its release edge.");
  }

  heldInputSimulation.Tick(heldUseInput);
  if (!heldInputInventory.GetSlot(0).IsEmpty ||
      heldInputSimulation.CreateItemUseSnapshot(heldInputPlayer).State.UseRevision != 2)
  {
    throw new InvalidOperationException(
      $"A release followed by a new press did not create one new item use. " +
      $"stack={heldInputInventory.GetSlot(0)}, " +
      $"revision={heldInputSimulation.CreateItemUseSnapshot(heldInputPlayer).State.UseRevision}, " +
      $"health={heldInputSimulation.CreateSnapshot().FindPlayer(heldInputPlayer).Health}");
  }
}

Console.WriteLine("PASS: non-auto-reuse held input remains edge-triggered");

using (DomeSimulation autoReuseSimulation = new())
{
  PlayerHandle autoReusePlayer = autoReuseSimulation.CreatePlayer(
    new SimulationVector(22.0f, 0.0f));
  InventoryComponent autoReuseInventory = autoReuseSimulation.GetInventory(autoReusePlayer);
  autoReuseInventory.SetSlot(0, new ItemStack(10, 2));
  SimulationInputBatch autoReuseInput = new(new PlayerInput(
    autoReusePlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true));

  autoReuseSimulation.Tick(autoReuseInput);
  ItemUseSnapshot firstAutoReuseSnapshot =
    autoReuseSimulation.CreateItemUseSnapshot(autoReusePlayer);
  ItemUsedEvent firstAutoReuseEvent = autoReuseSimulation.CreateItemUsedEvents().Single();
  if (firstAutoReuseSnapshot.State.UseRevision != 1 ||
      autoReuseInventory.GetSlot(0) != new ItemStack(10, 2))
  {
    throw new InvalidOperationException(
      "The AutoReuse fixture did not commit its initial use and ammunition consumption.");
  }

  long firstAutoReuseSequence = firstAutoReuseEvent.Sequence;
  autoReuseSimulation.Tick(autoReuseInput);
  autoReuseSimulation.Tick(autoReuseInput);
  autoReuseSimulation.Tick(autoReuseInput);
  ItemUseSnapshot secondAutoReuseSnapshot =
    autoReuseSimulation.CreateItemUseSnapshot(autoReusePlayer);
  IReadOnlyList<ItemUsedEvent> secondAutoReuseEvents = autoReuseSimulation.CreateItemUsedEvents();
  if (secondAutoReuseSnapshot.State.UseRevision != 2 ||
      autoReuseInventory.GetSlot(0) != new ItemStack(10, 2) ||
      secondAutoReuseEvents.Count != 1 ||
      autoReuseSimulation.LastTickTrace?.CommandCount != 1 ||
      secondAutoReuseEvents[0].Sequence <= firstAutoReuseSequence)
  {
    throw new InvalidOperationException(
      $"A held AutoReuse input did not schedule exactly one use after cooldown " +
      $"with a new sequence. " +
      $"revision={secondAutoReuseSnapshot.State.UseRevision}, " +
      $"stack={autoReuseInventory.GetSlot(0)}, events={secondAutoReuseEvents.Count}, " +
      $"commands={autoReuseSimulation.LastTickTrace?.CommandCount}, " +
      $"firstSequence={firstAutoReuseSequence}, " +
      $"secondSequence={secondAutoReuseEvents.FirstOrDefault().Sequence}");
  }

  autoReuseSimulation.Tick(new SimulationInputBatch(new PlayerInput(
    autoReusePlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: false)));
  autoReuseSimulation.Tick(new SimulationInputBatch());
  autoReuseSimulation.Tick(new SimulationInputBatch());
  if (autoReuseSimulation.CreateItemUseSnapshot(autoReusePlayer).State.UseRevision != 2 ||
      autoReuseInventory.GetSlot(0) != new ItemStack(10, 2) ||
      autoReuseSimulation.CreateItemUsedEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "Releasing an AutoReuse input did not stop later repeated uses.");
  }
}

using (DomeSimulation invalidAutoReuseSimulation = new())
{
  PlayerHandle invalidAutoReusePlayer = invalidAutoReuseSimulation.CreatePlayer(
    new SimulationVector(23.0f, 0.0f));
  InventoryComponent invalidAutoReuseInventory = invalidAutoReuseSimulation.GetInventory(
    invalidAutoReusePlayer);
  invalidAutoReuseInventory.SetSlot(0, new ItemStack(10, 100));
  SimulationInputBatch invalidAutoReuseInput = new(new PlayerInput(
    invalidAutoReusePlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true));

  invalidAutoReuseSimulation.Tick(invalidAutoReuseInput);
  invalidAutoReuseSimulation.Tick(invalidAutoReuseInput);
  invalidAutoReuseSimulation.Tick(invalidAutoReuseInput);
  invalidAutoReuseSimulation.Tick(invalidAutoReuseInput);
  if (invalidAutoReuseSimulation.CreateItemUseSnapshot(invalidAutoReusePlayer)
        .State.UseRevision != 0 ||
      invalidAutoReuseSimulation.CreateItemUsedEvents().Count != 0 ||
      invalidAutoReuseSimulation.LastTickTrace?.CommandCount != 0)
  {
    throw new InvalidOperationException(
      "An over-limit selected stack entered the repeated AutoReuse scheduling path.");
  }
}

Console.WriteLine("PASS: auto-reuse held input schedules one server-owned use per cooldown");

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
    combatProjectile.Damage != 14 || combatProjectile.IsSentry ||
    MathF.Abs(combatProjectile.Velocity.X - 5.0f) > 0.001f)
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

placementInventory.SetSlot(2, new ItemStack(5, 100));
long placementStackLimitRevision = placementInventory.Revision;
simulation.QueuePlaceItem(placementPlayer, 2, 41, 1);
simulation.Tick(new SimulationInputBatch());
if (placementInventory.GetSlot(2) != new ItemStack(5, 100) ||
    placementInventory.Revision != placementStackLimitRevision ||
    simulation.WorldGrid.GetTile(41, 1).IsActive)
{
  throw new InvalidOperationException(
    "Placement accepted an inventory stack above the item definition limit.");
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

using (DomeSimulation tileBoostPlacementSimulation = new())
{
  PlayerHandle tileBoostPlacementPlayer = tileBoostPlacementSimulation.CreatePlayer(
    new SimulationVector(100.0f, 100.0f));
  InventoryComponent tileBoostPlacementInventory =
    tileBoostPlacementSimulation.GetInventory(tileBoostPlacementPlayer);
  tileBoostPlacementInventory.SetSlot(0, new ItemStack(5, 1));
  tileBoostPlacementSimulation.QueuePlaceItem(tileBoostPlacementPlayer, 0, 113, 100);
  tileBoostPlacementSimulation.Tick(new SimulationInputBatch());
  if (tileBoostPlacementSimulation.WorldGrid.GetTile(113, 100).IsActive ||
      tileBoostPlacementInventory.GetSlot(0) != new ItemStack(5, 1) ||
      tileBoostPlacementSimulation.CreateInventoryChangedEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "A zero-boost placement was accepted outside the baseline interaction reach.");
  }

  tileBoostPlacementInventory.SetSlot(1, new ItemStack(11, 1));
  tileBoostPlacementSimulation.QueuePlaceItem(tileBoostPlacementPlayer, 1, 113, 100);
  tileBoostPlacementSimulation.Tick(new SimulationInputBatch());
  if (tileBoostPlacementSimulation.WorldGrid.GetTile(113, 100) !=
        new WorldTile(IsActive: true, Type: 12) ||
      !tileBoostPlacementInventory.GetSlot(1).IsEmpty ||
      tileBoostPlacementSimulation.CreateInventoryChangedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A placement item's TileBoost did not extend authoritative interaction reach.");
  }

  tileBoostPlacementInventory.SetSlot(2, new ItemStack(12, 1));
  tileBoostPlacementSimulation.QueuePlaceItem(tileBoostPlacementPlayer, 2, 113, 100);
  tileBoostPlacementSimulation.Tick(new SimulationInputBatch());
  if (tileBoostPlacementSimulation.WorldGrid.GetTile(113, 100) !=
        new WorldTile(IsActive: true, Type: 12, WallType: 7) ||
      !tileBoostPlacementInventory.GetSlot(2).IsEmpty ||
      tileBoostPlacementSimulation.CreateInventoryChangedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A wall placement item's TileBoost did not extend authoritative interaction reach.");
  }

  tileBoostPlacementInventory.SetSlot(3, new ItemStack(1, 1));
  tileBoostPlacementSimulation.QueuePlaceItem(tileBoostPlacementPlayer, 3, 100, 101);
  tileBoostPlacementSimulation.Tick(new SimulationInputBatch());
  if (tileBoostPlacementInventory.GetSlot(3) != new ItemStack(1, 1) ||
      tileBoostPlacementSimulation.WorldGrid.GetTile(100, 101).IsActive ||
      tileBoostPlacementSimulation.CreateInventoryChangedEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "An item without a placement definition mutated state through the placement path.");
  }

  tileBoostPlacementInventory.SetSlot(4, new ItemStack(ushort.MaxValue, 1));
  tileBoostPlacementSimulation.QueuePlaceItem(tileBoostPlacementPlayer, 4, 100, 102);
  tileBoostPlacementSimulation.Tick(new SimulationInputBatch());
  if (tileBoostPlacementInventory.GetSlot(4) != new ItemStack(ushort.MaxValue, 1) ||
      tileBoostPlacementSimulation.WorldGrid.GetTile(100, 102).IsActive ||
      tileBoostPlacementSimulation.CreateInventoryChangedEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "An unknown item type mutated state through the placement path.");
  }
}

Console.WriteLine("PASS: item placement TileBoost extends authoritative interaction reach");

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
InventoryComponent forgedEquipmentStatsInventory = new();
forgedEquipmentStatsInventory.SetSlot(0, new ItemStack(4, 2));
EquipmentStateCollectionComponent forgedEquipmentStatsStates = new();
forgedEquipmentStatsStates.Add(
  new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Head,
    0,
    false));
DefenseComponent forgedDefense = new();
HealthRegenerationComponent forgedRegen = new();
ManaComponent forgedMana = new(20, 20);
new EquipmentStatSystem().Apply(
  ref forgedDefense,
  ref forgedRegen,
  ref forgedMana,
  forgedEquipmentStatsStates,
  forgedEquipmentStatsInventory,
  new ItemDefinitionRegistry([
    new ItemDefinition(
      4,
      1,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 5))]));
if (forgedDefense.Value != 0)
{
  throw new InvalidOperationException(
    "Equipment stats accepted an inventory stack above the item definition limit.");
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

equipmentInventory.SetSlot(1, new ItemStack(4, 2));
simulation.QueueEquipItem(equipmentPlayer, 1, isVanity: false);
simulation.Tick(new SimulationInputBatch());
if (simulation.CreateEquipmentSnapshot(equipmentPlayer).Slots.Count != 0 ||
    equipmentInventory.GetSlot(1) != new ItemStack(4, 2))
{
  throw new InvalidOperationException(
    "Equipment accepted an inventory stack above the item definition limit.");
}

equipmentInventory.SetSlot(1, new ItemStack(4, 1));
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

ItemDefinitionRegistry manaEquipmentDefinitions = new([
  new ItemDefinition(
    9,
    1,
    Equipment: new ItemEquipmentDefinition(
      ItemEquipmentSlot.Accessory,
      ManaIncrease: 20))]);
InventoryComponent manaEquipmentInventory = new();
manaEquipmentInventory.SetSlot(0, new ItemStack(9, 1));
EquipmentStateCollectionComponent manaEquipmentStates = new();
manaEquipmentStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
  ItemEquipmentSlot.Accessory,
  0,
  false));
DefenseComponent manaEquipmentDefense = new();
HealthRegenerationComponent manaEquipmentRegeneration = new();
ManaComponent manaEquipmentMana = new(20, 20);
new EquipmentStatSystem().Apply(
  ref manaEquipmentDefense,
  ref manaEquipmentRegeneration,
  ref manaEquipmentMana,
  manaEquipmentStates,
  manaEquipmentInventory,
  manaEquipmentDefinitions);
if (manaEquipmentMana.Maximum != 40 || manaEquipmentMana.EquipmentIncrease != 20)
{
  throw new InvalidOperationException(
    "Equipped ManaIncrease was not applied to the authoritative mana maximum.");
}

manaEquipmentMana.Current = 35;
manaEquipmentStates.Remove(ItemEquipmentSlot.Accessory);
new EquipmentStatSystem().Apply(
  ref manaEquipmentDefense,
  ref manaEquipmentRegeneration,
  ref manaEquipmentMana,
  manaEquipmentStates,
  manaEquipmentInventory,
  manaEquipmentDefinitions);
if (manaEquipmentMana.Maximum != 20 || manaEquipmentMana.Current != 20 ||
    manaEquipmentMana.EquipmentIncrease != 0)
{
  throw new InvalidOperationException(
    "Unequipping ManaIncrease did not restore and clamp the authoritative mana maximum.");
}

Console.WriteLine("PASS: equipped ManaIncrease updates and safely restores player mana maximum");

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

modifierInventory.SetSlot(1, new ItemStack(3, 2));
long variantStackLimitRevision = modifierInventory.Revision;
simulation.QueueApplyItemVariant(
  modifierPlayer,
  1,
  new ItemVariantDefinition(9, 3, 9));
simulation.Tick(new SimulationInputBatch());
if (modifierInventory.GetSlot(1) != new ItemStack(3, 2) ||
    modifierInventory.Revision != variantStackLimitRevision ||
    simulation.CreateInventoryChangedEvents().Count != 0)
{
  throw new InvalidOperationException(
    "A variant command bypassed the replacement item stack limit.");
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

commandInventory.SetSlot(2, new ItemStack(1, 100));
long forgedDropRevision = commandInventory.Revision;
simulation.QueueDropItem(inventoryCommandPlayer, 2, 1);
simulation.Tick(new SimulationInputBatch());
if (commandInventory.GetSlot(2) != new ItemStack(1, 100) ||
    commandInventory.Revision != forgedDropRevision)
{
  throw new InvalidOperationException(
    "An over-limit item drop was consumed before world-item validation.");
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

PlayerPersistentState overLimitAccount = CreatePersistentState(
  "5a4a6f5e-6a0c-4e1d-9eb7-9e5dd0e65021",
  firstItemType: 1,
  firstItemStack: 100,
  retainedItemType: 0,
  retainedItemStack: 0);
Terraria.Dome.Simulation.DomeSimulation overLimitSimulation = new();
PlayerHandle overLimitPlayer = overLimitSimulation.CreatePlayer(
  overLimitAccount,
  new SimulationVector(21.0f, 0.0f));
if (!overLimitSimulation.GetInventory(overLimitPlayer).GetSlot(0).IsEmpty)
{
  throw new InvalidOperationException(
    "Player restore silently truncated an over-limit persistent item into the inventory.");
}

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

DomeSimulation restoredPersistenceSimulation = new(persistedSimulation);
if (restoredPersistenceSimulation.CreateItemReplicationSnapshots().Count !=
    persistedSimulation.WorldItems.Count)
{
  throw new InvalidOperationException(
    "A valid world-item persistence snapshot did not restore its item set.");
}

ItemReplicationSnapshot maxReplicationIdItem = persistedSimulation.WorldItems.Count == 0
  ? new ItemReplicationSnapshot(
    int.MaxValue,
    new ItemStack(1, 1),
    new SimulationVector(0.0f, 0.0f),
    true,
    1,
    new WorldSectionCoordinates(0, 0))
  : persistedSimulation.WorldItems[0] with { ReplicationId = int.MaxValue };
List<ItemReplicationSnapshot> invalidReplicationItems = [maxReplicationIdItem];
DomeSimulationSnapshot invalidReplicationSnapshot = new(
  persistedSimulation.World,
  persistedSimulation.Npcs,
  invalidReplicationItems,
  persistedSimulation.TickNumber,
  persistedSimulation.PlayerAccounts,
  persistedSimulation.Chests,
  persistedSimulation.Signs,
  persistedSimulation.TileEntities,
  persistedSimulation.OpaqueCompatibilityRecords,
  persistedSimulation.Clock,
  persistedSimulation.NpcStates,
  persistedSimulation.WorldRules,
  persistedSimulation.Progression,
  persistedSimulation.WorldEventRandomState,
  persistedSimulation.WorldTimeRate);
bool rejectedReplicationIdOverflow = false;
try
{
  _ = new DomeSimulation(invalidReplicationSnapshot);
}
catch (ArgumentOutOfRangeException)
{
  rejectedReplicationIdOverflow = true;
}

if (!rejectedReplicationIdOverflow)
{
  throw new InvalidOperationException(
    "Persistence accepted a world-item replication ID that would overflow the next allocator.");
}

Console.WriteLine(
  "PASS: world-item persistence rejects max replication IDs before allocator overflow");

ItemReplicationSnapshot overLimitPersistenceItem = new(
  1,
  new ItemStack(1, 100),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  new WorldSectionCoordinates(0, 0));
DomeSimulationSnapshot overLimitPersistenceSnapshot = new(
  persistedSimulation.World,
  persistedSimulation.Npcs,
  [overLimitPersistenceItem],
  persistedSimulation.TickNumber,
  persistedSimulation.PlayerAccounts,
  persistedSimulation.Chests,
  persistedSimulation.Signs,
  persistedSimulation.TileEntities,
  persistedSimulation.OpaqueCompatibilityRecords,
  persistedSimulation.Clock,
  persistedSimulation.NpcStates,
  persistedSimulation.WorldRules,
  persistedSimulation.Progression,
  persistedSimulation.WorldEventRandomState,
  persistedSimulation.WorldTimeRate);
bool rejectedOverLimitPersistenceStack = false;
try
{
  _ = new DomeSimulation(overLimitPersistenceSnapshot);
}
catch (ArgumentOutOfRangeException)
{
  rejectedOverLimitPersistenceStack = true;
}

if (!rejectedOverLimitPersistenceStack)
{
  throw new InvalidOperationException(
    "Persistence accepted an over-limit world-item stack through the direct snapshot path.");
}

Console.WriteLine("PASS: world-item persistence validates stack and instance value domains");

WorldItemComponent invalidStatePickupItem = new(
  100,
  new ItemStack(1, 1),
  new SimulationVector(0.0f, 0.0f),
  true,
  1,
  new WorldSectionCoordinates(0, 0),
  Terraria.Dome.Simulation.Items.Components.ItemWorldStateComponent.Active(0),
  new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
    NameOverride: new string('x', 201)));
if (new WorldItemPickupSystem().TryPickup(
      ref invalidStatePickupItem,
      new PickupWorldItemCommand(new PlayerHandle(1), 100),
      new SimulationVector(0.0f, 0.0f),
      new InventoryComponent(),
      definitions,
      5.0f,
      out _,
      out _) ||
    invalidStatePickupItem.Stack != new ItemStack(1, 1))
{
  throw new InvalidOperationException(
    "World item pickup propagated invalid instance state instead of rejecting it.");
}

ItemDefinition legacyDefinition = LegacyItemDefinitionAdapter.ToDefinition(
  new LegacyItemDefinitionRecord(
    ItemType: 6,
    MaxStack: 20,
    HealthRestore: 15,
    UseCooldownTicks: 4,
    NameKey: "Legacy.Item",
    IsMaterial: true,
    IsQuestItem: true,
    UniqueStack: true,
    ExpertOnly: true,
    Expert: true,
    IsShopCurrency: true,
    IsShopItem: true,
    ChlorophyteExtractinatorConsumable: true,
    Buy: true,
    BuyOnce: true,
    Alpha: 96,
    Scale: 0.5f,
    Width: 18,
    Height: 22,
    Value: 1250,
    Rarity: 3,
    UseStyle: 1,
    UseTime: 12,
    UseAnimation: 24,
    ReuseDelayTicks: 9,
    Consumable: true,
    Channel: true,
    AutoReuse: true,
    UseTurn: true,
    ShootsEveryUse: true,
    NoUseGraphic: true,
    Potion: true,
    Damage: 42,
    BonusTagDamage: 5,
    Knockback: 3.5f,
    CriticalChance: 7,
    ArmorPenetration: 2,
    DamageClass: ItemDamageClass.Ranged,
    Ranged: true,
    Flame: true,
    Mech: true,
    ProjectileType: 9,
    ProjectileSpeed: 6.5f,
    ShootType: 9,
    ShootSpeed: 6.5f,
    WallType: 7,
    PlaceStyle: 2,
    TileBoost: 3,
    ManaRestore: 8,
    ManaCost: 4,
    BuffType: 12,
    BuffDurationTicks: 60,
    AmmoType: 10,
    NotAmmo: true,
    ConsumesAmmo: true,
    Staff: true,
    Claw: true,
    EquipmentSlot: ItemEquipmentSlot.Head,
    Defense: 5,
    LifeRegen: 2,
    ManaIncrease: 20,
    HeadSlot: 3,
    BodySlot: 4,
    LegSlot: 5,
    HandOnSlot: 6,
    HandOffSlot: 7,
    BackSlot: 8,
    FrontSlot: 9,
    ShoeSlot: 10,
    WaistSlot: 11,
    WingSlot: 12,
    ShieldSlot: 13,
    NeckSlot: 14,
    FaceSlot: 15,
    BalloonSlot: 16,
    BeardSlot: 17,
    VoiceSlot: 18,
    HasVanityEffects: true,
    HairDye: 6,
    Paint: 3,
    PaintCoating: 2,
    Accessory: true,
    Vanity: true,
    Social: true,
    PickPower: 55,
    AxePower: 12,
    HammerPower: 80,
    TileWandPower: 4,
    FishingPolePower: 30,
    BaitPower: 25,
    MakeNpc: 17,
    MountType: 2,
    Sentry: true));
if (legacyDefinition.ItemType != 6 || legacyDefinition.StackLimit != 20 ||
    legacyDefinition.HealthRestore != 15 || legacyDefinition.UseCooldownTicks != 4 ||
    legacyDefinition.Identity?.NameKey != "Legacy.Item" ||
    legacyDefinition.Identity?.IsMaterial != true ||
    legacyDefinition.Identity?.IsQuestItem != true ||
    legacyDefinition.Identity?.UniqueStack != true ||
    legacyDefinition.Identity?.ExpertOnly != true ||
    legacyDefinition.Identity?.Expert != true ||
    legacyDefinition.Identity?.IsShopCurrency != true || legacyDefinition.Identity?.IsShopItem != true ||
    legacyDefinition.Identity?.Buy != true ||
    legacyDefinition.Identity?.BuyOnce != true || legacyDefinition.Alpha != 96 ||
    legacyDefinition.Extractinator?.ExtractionMode != 4 ||
    legacyDefinition.Scale != 0.5f || legacyDefinition.Width != 18 ||
    legacyDefinition.Height != 22 || legacyDefinition.Value != 1250 || legacyDefinition.Rarity != 3)
{
  throw new InvalidOperationException(
    "Legacy item definition data was not mapped to ECS metadata.");
}

if (!legacyDefinition.IsSentry)
{
  throw new InvalidOperationException(
    "Legacy item sentry metadata was not mapped to immutable item metadata.");
}

if (legacyDefinition.Use?.UseStyle != 1 || legacyDefinition.Use?.UseTime != 12 ||
    legacyDefinition.Use?.UseAnimation != 24 || legacyDefinition.Use?.ReuseDelayTicks != 9 ||
    legacyDefinition.Use?.Consumable != true ||
    legacyDefinition.Use?.Channel != true || legacyDefinition.Use?.AutoReuse != true ||
    legacyDefinition.Use?.UseTurn != true || legacyDefinition.Use?.ShootsEveryUse != true)
{
  throw new InvalidOperationException(
    "Legacy item use metadata was not mapped to the immutable use definition.");
}

if (legacyDefinition.Combat?.Damage != 42 || legacyDefinition.Combat?.Knockback != 3.5f ||
    legacyDefinition.Combat?.BonusTagDamage != 5 ||
    legacyDefinition.Combat?.CriticalChance != 7 ||
    legacyDefinition.Combat?.ArmorPenetration != 2 ||
    legacyDefinition.Combat?.DamageClass != ItemDamageClass.Ranged ||
    legacyDefinition.Combat?.Flame != true || legacyDefinition.Combat?.Mech != true ||
    legacyDefinition.Combat?.NotAmmo != true ||
    legacyDefinition.Combat?.Staff != true || legacyDefinition.Combat?.Claw != true ||
    legacyDefinition.Combat?.ProjectileType != 9 ||
    legacyDefinition.Combat?.ProjectileSpeed != 6.5f)
{
  throw new InvalidOperationException(
    "Legacy item combat metadata was not mapped to the immutable combat definition.");
}

ItemDefinition staffOnlyDefinition = LegacyItemDefinitionAdapter.ToDefinition(
  new LegacyItemDefinitionRecord(ItemType: 7, MaxStack: 1, Staff: true));
ItemDefinition clawOnlyDefinition = LegacyItemDefinitionAdapter.ToDefinition(
  new LegacyItemDefinitionRecord(ItemType: 8, MaxStack: 1, Claw: true));
if (staffOnlyDefinition.Combat?.Staff != true || clawOnlyDefinition.Combat?.Claw != true)
{
  throw new InvalidOperationException(
    "Legacy staff and claw flags require a combat definition.");
}

if (legacyDefinition.Placement?.WallType != 7 || legacyDefinition.Placement?.PlaceStyle != 2 ||
    legacyDefinition.Placement?.TileBoost != 3)
{
  throw new InvalidOperationException(
    "Legacy item placement metadata was not mapped to the immutable placement definition.");
}

if (legacyDefinition.Use?.ManaRestore != 8 || legacyDefinition.Use?.ManaCost != 4 ||
    legacyDefinition.Recovery?.BuffType != 12 ||
    legacyDefinition.Recovery?.BuffDurationTicks != 60)
{
  throw new InvalidOperationException(
    "Legacy item recovery metadata was not mapped to immutable definitions.");
}

if (legacyDefinition.Use?.AmmoType != 10 || !legacyDefinition.Use.Value.ConsumesAmmo ||
    legacyDefinition.Combat?.AmmoType != 10 || !legacyDefinition.Combat.Value.ConsumesAmmo)
{
  throw new InvalidOperationException(
    "Legacy item ammunition metadata was not mapped to immutable use/combat definitions.");
}

if (legacyDefinition.Use?.ShootType != 9 || legacyDefinition.Use?.ShootSpeed != 6.5f)
{
  throw new InvalidOperationException(
    "Legacy item shooting metadata was not mapped to the immutable use definition.");
}

if (legacyDefinition.Use?.NoUseGraphic != true)
{
  throw new InvalidOperationException(
    "Legacy item use-graphic metadata was not mapped to the immutable use definition.");
}

if (legacyDefinition.Use?.Potion != true)
{
  throw new InvalidOperationException(
    "Legacy item potion metadata was not mapped to the immutable use definition.");
}

if (legacyDefinition.Equipment?.Slot != ItemEquipmentSlot.Head ||
    legacyDefinition.Equipment?.Defense != 5 || legacyDefinition.Equipment?.LifeRegen != 2 ||
    legacyDefinition.Equipment?.ManaIncrease != 20 ||
    legacyDefinition.Equipment?.HeadSlot != 3 || legacyDefinition.Equipment?.BodySlot != 4 ||
    legacyDefinition.Equipment?.LegSlot != 5 ||
    legacyDefinition.Equipment?.HandOnSlot != 6 || legacyDefinition.Equipment?.HandOffSlot != 7 ||
    legacyDefinition.Equipment?.BackSlot != 8 || legacyDefinition.Equipment?.FrontSlot != 9 ||
    legacyDefinition.Equipment?.ShoeSlot != 10 || legacyDefinition.Equipment?.WaistSlot != 11 ||
    legacyDefinition.Equipment?.WingSlot != 12 || legacyDefinition.Equipment?.ShieldSlot != 13 ||
    legacyDefinition.Equipment?.NeckSlot != 14 || legacyDefinition.Equipment?.FaceSlot != 15 ||
    legacyDefinition.Equipment?.BalloonSlot != 16 || legacyDefinition.Equipment?.BeardSlot != 17 ||
    legacyDefinition.Equipment?.VoiceSlot != 18 ||
    legacyDefinition.Equipment?.HasVanityEffects != true ||
    legacyDefinition.Equipment?.Accessory != true || legacyDefinition.Equipment?.Vanity != true ||
    legacyDefinition.Equipment?.Social != true)
{
  throw new InvalidOperationException(
    "Legacy item equipment metadata was not mapped to the immutable equipment definition.");
}

if (legacyDefinition.Appearance?.HairDye != 6 || legacyDefinition.Appearance?.Paint != 3 ||
    legacyDefinition.Appearance?.PaintCoating != 2 ||
    !legacyDefinition.Appearance.Value.PaintOrCoating)
{
  throw new InvalidOperationException(
    "Legacy item appearance metadata was not mapped to the immutable appearance definition.");
}

if (new ItemAppearanceDefinition().PaintOrCoating)
{
  throw new InvalidOperationException(
    "An item without paint or coating was incorrectly classified as painted.");
}

ItemEquipmentDefinition emptyVisualSlots = new(ItemEquipmentSlot.None, HandOnSlot: -1, WingSlot: -1);
if (emptyVisualSlots.HandOnSlot != -1 || emptyVisualSlots.WingSlot != -1 ||
    emptyVisualSlots.VoiceSlot != 0)
{
  throw new InvalidOperationException(
    "Equipment visual slots did not preserve their legacy empty sentinels.");
}

if (legacyDefinition.Tools?.PickPower != 55 || legacyDefinition.Tools?.AxePower != 12 ||
    legacyDefinition.Tools?.HammerPower != 80 || legacyDefinition.Tools?.TileWandPower != 4)
{
  throw new InvalidOperationException(
    "Legacy item tool metadata was not mapped to the immutable tool definition.");
}

if (legacyDefinition.Gathering?.FishingPolePower != 30 ||
    legacyDefinition.Gathering?.BaitPower != 25)
{
  throw new InvalidOperationException(
    "Legacy item gathering metadata was not mapped to the immutable gathering definition.");
}

if (legacyDefinition.Summoning?.NpcType != 17 ||
    legacyDefinition.Summoning?.MountType != 2)
{
  throw new InvalidOperationException(
    "Legacy item summon metadata was not mapped to the immutable summoning definition.");
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Alpha: 256));
  throw new InvalidOperationException("Legacy item adapter accepted an out-of-range alpha.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, ShootSpeed: float.PositiveInfinity));
  throw new InvalidOperationException("Legacy item adapter accepted a non-finite shoot speed.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Defense: -1));
  throw new InvalidOperationException("Legacy item adapter accepted negative defense.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, BaitPower: -1));
  throw new InvalidOperationException("Legacy item adapter accepted negative bait power.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, MountType: -2));
  throw new InvalidOperationException("Legacy item adapter accepted an invalid mount type.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, ManaIncrease: -1));
  throw new InvalidOperationException("Legacy item adapter accepted negative mana increase.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, HeadSlot: -2));
  throw new InvalidOperationException("Legacy item adapter accepted an invalid head slot.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, HairDye: -2));
  throw new InvalidOperationException("Legacy item adapter accepted an invalid hair dye.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Melee: true, Magic: true));
  throw new InvalidOperationException("Legacy item adapter accepted conflicting damage classes.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, ShootsEveryUse: true));
  throw new InvalidOperationException(
    "Legacy item adapter accepted every-use shooting without a projectile.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, BuyOnce: true));
  throw new InvalidOperationException(
    "Legacy item adapter accepted buy-once without buy permission.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, TileType: 1, WallType: 1));
  throw new InvalidOperationException(
    "Legacy item adapter accepted tile and wall placement together.");
}
catch (ArgumentException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, AmmoType: 65536));
  throw new InvalidOperationException("Legacy item adapter accepted an out-of-range ammo type.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, ManaCost: -1));
  throw new InvalidOperationException("Legacy item adapter accepted a negative mana cost.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, TileBoost: -1));
  throw new InvalidOperationException("Legacy item adapter accepted a negative tile boost.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Damage: -1));
  throw new InvalidOperationException("Legacy item adapter accepted negative damage.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, UseTime: -1));
  throw new InvalidOperationException("Legacy item adapter accepted a negative use time.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, ReuseDelayTicks: -1));
  throw new InvalidOperationException("Legacy item adapter accepted a negative reuse delay.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Width: -1));
  throw new InvalidOperationException("Legacy item adapter accepted a negative width.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(6, 20, Scale: float.NaN));
  throw new InvalidOperationException("Legacy item adapter accepted a non-finite scale.");
}
catch (ArgumentOutOfRangeException)
{
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

if (LegacyItemImportAdapter.TryImport(
      new PlayerPersistentItem(10, 21, 0, 6, false, false),
      legacyDefinition,
      out _))
{
  throw new InvalidOperationException(
    "Legacy player item import silently truncated a stack above the definition limit.");
}

if (LegacyItemImportAdapter.TryImport(
      new PlayerPersistentItem(PlayerPersistentState.ItemSlotCount, 1, 0, 6, false, false),
      legacyDefinition,
      out _))
{
  throw new InvalidOperationException(
    "Legacy player item import accepted a slot outside the persistent inventory range.");
}

if (LegacyItemImportAdapter.TryImport(
      new PlayerPersistentItem(
        10,
        1,
        0,
        6,
        false,
        false,
        NameOverride: new string('x', 201)),
      legacyDefinition,
      out _))
{
  throw new InvalidOperationException(
    "Legacy player item import accepted an oversized name override.");
}

PlayerPersistentItem[] oversizedNameItems = new PlayerPersistentItem[PlayerPersistentState.ItemSlotCount];
for (int slotId = 0; slotId < oversizedNameItems.Length; slotId++)
{
  oversizedNameItems[slotId] = new PlayerPersistentItem(slotId, 0, 0, 0, false, false);
}
oversizedNameItems[10] = new PlayerPersistentItem(
  10,
  1,
  0,
  6,
  false,
  false,
  NameOverride: new string('x', 201));
try
{
  _ = new PlayerPersistentState(
    "00000000-0000-0000-0000-000000000001",
    new PlayerPersistentProfile("Player"),
    100,
    100,
    20,
    20,
    [],
    0,
    0,
    oversizedNameItems);
  throw new InvalidOperationException(
    "Player persistence state accepted an oversized item name override.");
}
catch (ArgumentOutOfRangeException)
{
}

oversizedNameItems[10] = new PlayerPersistentItem(10, 1, 0, ushort.MaxValue + 1, false, false);
try
{
  _ = new PlayerPersistentState(
    "00000000-0000-0000-0000-000000000001",
    new PlayerPersistentProfile("Player"),
    100,
    100,
    20,
    20,
    [],
    0,
    0,
    oversizedNameItems);
  throw new InvalidOperationException(
    "Player persistence state accepted an item type outside the protocol range.");
}
catch (ArgumentOutOfRangeException)
{
}

oversizedNameItems[10] = new PlayerPersistentItem(10, 1, 0, 0, false, false);
try
{
  _ = new PlayerPersistentState(
    "00000000-0000-0000-0000-000000000001",
    new PlayerPersistentProfile("Player"),
    100,
    100,
    20,
    20,
    [],
    0,
    0,
    oversizedNameItems);
  throw new InvalidOperationException(
    "Player persistence state accepted a positive stack for the empty item type.");
}
catch (ArgumentOutOfRangeException)
{
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

if (LegacyItemDropAdapter.TryCreateCommand(
      legacyDrop,
      quantity: 1,
      new SimulationVector(float.NaN, 0.0f),
      new WorldSectionCoordinates(0, 0),
      spawnSource: 1,
      out _))
{
  throw new InvalidOperationException(
    "Legacy item drop adapter accepted a non-finite world position.");
}

ItemDefinitionRegistry legacyDropDefinitions = new([new ItemDefinition(6, 2)]);
if (!LegacyItemDropAdapter.TryCreateCommand(
      legacyDrop,
      2,
      new SimulationVector(3.0f, 4.0f),
      new WorldSectionCoordinates(0, 0),
      1,
      legacyDropDefinitions,
      out CreateWorldItemCommand authoritativeDropCommand) ||
    authoritativeDropCommand.Stack != new ItemStack(6, 2))
{
  throw new InvalidOperationException(
    "Legacy item drop adapter rejected a valid authoritative Definition range.");
}

if (LegacyItemDropAdapter.TryCreateCommand(
      new LegacyItemDropRecord(99),
      1,
      new SimulationVector(3.0f, 4.0f),
      new WorldSectionCoordinates(0, 0),
      1,
      legacyDropDefinitions,
      out _) ||
    LegacyItemDropAdapter.TryCreateCommand(
      new LegacyItemDropRecord(6, 1, 3),
      3,
      new SimulationVector(3.0f, 4.0f),
      new WorldSectionCoordinates(0, 0),
      1,
      legacyDropDefinitions,
      out _))
{
  throw new InvalidOperationException(
    "Legacy item drop adapter emitted an unknown or over-limit item.");
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

VerifyItemNpcSummonCommit();
VerifyItemSentryProjectileCommit();
VerifyItemSentryUsesLegacyLifetime();
VerifyOrdinaryProjectileKeepsDefinitionLifetime();
VerifyItemSentryOverlapAdmission();
VerifyItemSentryNonOverlapAndOrdinaryUse();
VerifyExpiredSentryDoesNotBlockPlacement();
VerifyItemSentryRuntimeLimit();
VerifyItemSentryEventActiveRuntimeReconcile();
VerifyItemSentryPlayerAuthority();
VerifyItemSentryEquipmentBonus();
VerifyLegacySentryEquipmentIdMapping();
VerifyLegacySentryBuff348Capacity();
VerifyLegacySentryArmorSetCapacity();
VerifyLegacyNoMeleeSleepWake();
VerifyLegacyFishingPoleBobberAdmission();
VerifyLegacyDd2SummonSentryAuthority();
VerifyShopPurchaseTransaction();
VerifyShopPurchaseTickAuthority();
VerifyShopCustomPriceOverride();
VerifyShopSpecialCurrencyPayment();
VerifyShopPurchaseOnPurchaseCleanup();
VerifyShopCatalogCleanupConsumer();
VerifyShopCatalogTickConsumer();
VerifyShopCatalogGeneratedRefresh();
VerifyCartTrackPlacementOwner();
VerifyPotionDelayAuthority();
VerifyLegacyFlaskDurationOwner();

static void VerifyItemNpcSummonCommit()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemNpcSummonType, 1));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  if (!inventory.GetSlot(0).IsEmpty || simulation.NpcCount != 0 ||
      simulation.CreateInventoryChangedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "A makeNPC item did not consume exactly once before its pending NPC commit.");
  }

  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<NpcReplicationSnapshot> summons = simulation.CreateNpcReplicationSnapshots();
  if (summons.Count != 1 || !summons[0].IsActive ||
      summons[0].DefinitionId != DomeSimulation.FixtureNpcType ||
      summons[0].SpawnSource !=
      Terraria.Dome.Simulation.Npc.Components.NpcSpawnSource.Command)
  {
    throw new InvalidOperationException(
      "A makeNPC item did not commit its authoritative NPC spawn on the next tick.");
  }

  Console.WriteLine("PASS: item makeNPC metadata schedules one authoritative NPC spawn");
}

static void VerifyItemSentryProjectileCommit()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(12.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));

  ProjectileReplicationSnapshot projectile = simulation.CreateProjectileReplicationSnapshots()
    .SingleOrDefault(entry => entry.Owner == player && entry.IsActive);
  Arch.Core.QueryDescription sentryQuery = new Arch.Core.QueryDescription()
    .WithAll<ProjectileSentryComponent, ProjectileOwnerComponent>();
  bool hasSentryComponent = false;
  simulation.World.Query(
    in sentryQuery,
    (Arch.Core.Entity entity, ref ProjectileOwnerComponent owner) =>
    {
      if (owner.Owner == player)
      {
        hasSentryComponent = true;
      }
    });
  if (projectile.ReplicationId == 0 || projectile.ProjectileType != 2 || !projectile.IsSentry ||
      !hasSentryComponent ||
      !inventory.GetSlot(0).IsEmpty || simulation.CreateInventoryChangedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      $"A sentry item did not create one authoritative sentry projectile atomically " +
      $"(id={projectile.ReplicationId}, type={projectile.ProjectileType}, " +
      $"isSentry={projectile.IsSentry}, component={hasSentryComponent}, " +
      $"empty={inventory.GetSlot(0).IsEmpty}, " +
      $"events={simulation.CreateInventoryChangedEvents().Count}).");
  }

  Console.WriteLine("PASS: item sentry metadata creates an authoritative sentry projectile");
}

static void VerifyItemSentryUsesLegacyLifetime()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(12.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  simulation.Tick(new SimulationInputBatch());

  ProjectileReplicationSnapshot projectile = simulation.CreateProjectileReplicationSnapshots()
    .Single(entry => entry.Owner == player && entry.IsActive && entry.IsSentry);
  int defaultLifetimeTicks = 0;
  int remainingLifetimeTicks = 0;
  bool foundSentry = false;
  Arch.Core.QueryDescription sentryQuery = new Arch.Core.QueryDescription()
    .WithAll<ProjectileSentryComponent, ProjectileOwnerComponent, ProjectileDefinitionComponent,
      ProjectileLifetimeComponent>();
  simulation.World.Query(
    in sentryQuery,
    (Arch.Core.Entity entity, ref ProjectileOwnerComponent owner,
      ref ProjectileDefinitionComponent definition, ref ProjectileLifetimeComponent lifetime) =>
    {
      if (owner.Owner == player)
      {
        foundSentry = true;
        defaultLifetimeTicks = definition.DefaultLifetimeTicks;
        remainingLifetimeTicks = lifetime.RemainingTicks;
      }
    });
  if (!foundSentry || projectile.RemainingLifetime != 35999 ||
      defaultLifetimeTicks != 36000 || remainingLifetimeTicks != 35999)
  {
    throw new InvalidOperationException(
      $"Sentry lifetime did not use the source-backed 36000-tick contract " +
      $"(snapshot={projectile.RemainingLifetime}, default={defaultLifetimeTicks}, " +
      $"remaining={remainingLifetimeTicks}).");
  }

  Console.WriteLine("PASS: item sentry uses the source-backed 36000-tick lifetime");
}

static void VerifyOrdinaryProjectileKeepsDefinitionLifetime()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(8, 1));

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));

  ProjectileReplicationSnapshot initial = simulation.CreateProjectileReplicationSnapshots()
    .Single(projectile => projectile.Owner == player && projectile.IsActive);
  if (initial.IsSentry || initial.RemainingLifetime != 1200)
  {
    throw new InvalidOperationException(
      $"An ordinary projectile did not retain its definition lifetime " +
      $"(isSentry={initial.IsSentry}, lifetime={initial.RemainingLifetime}).");
  }

  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot advanced = simulation.CreateProjectileReplicationSnapshots()
    .Single(projectile => projectile.Owner == player && projectile.IsActive);
  if (advanced.RemainingLifetime != 1199)
  {
    throw new InvalidOperationException(
      $"An ordinary projectile did not advance from its definition lifetime " +
      $"(lifetime={advanced.RemainingLifetime}).");
  }

  Console.WriteLine("PASS: ordinary projectiles retain the definition lifetime");
}

static void VerifyItemSentryOverlapAdmission()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle firstPlayer = simulation.CreatePlayer(new SimulationVector(18.0f, 0.0f));
  PlayerHandle secondPlayer = simulation.CreatePlayer(new SimulationVector(18.0f, 0.0f));
  InventoryComponent firstInventory = simulation.GetInventory(firstPlayer);
  InventoryComponent secondInventory = simulation.GetInventory(secondPlayer);
  firstInventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));
  secondInventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));
  long secondInventoryRevision = secondInventory.Revision;

  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true),
    new PlayerInput(
      secondPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));

  ProjectileReplicationSnapshot[] activeSentries = simulation.CreateProjectileReplicationSnapshots()
    .Where(projectile => projectile.IsActive && projectile.IsSentry)
    .ToArray();
  if (activeSentries.Length != 1 || !firstInventory.GetSlot(0).IsEmpty ||
      secondInventory.GetSlot(0) != new ItemStack(DomeSimulation.FixtureItemSentryType, 1) ||
      secondInventory.Revision != secondInventoryRevision ||
      simulation.CreateItemUseSnapshot(secondPlayer).State.UseRevision != 0 ||
      simulation.CreateInventoryChangedEvents().Count != 1 ||
      simulation.CreateItemUsedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "An overlapping sentry use mutated inventory or use state after the first " +
      "sentry was admitted.");
  }

  Console.WriteLine("PASS: overlapping sentry admission is atomic and deterministic");
}

static void VerifyItemSentryNonOverlapAndOrdinaryUse()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle firstPlayer = simulation.CreatePlayer(new SimulationVector(26.0f, 0.0f));
  PlayerHandle secondPlayer = simulation.CreatePlayer(new SimulationVector(29.0f, 0.0f));
  InventoryComponent firstInventory = simulation.GetInventory(firstPlayer);
  InventoryComponent secondInventory = simulation.GetInventory(secondPlayer);
  firstInventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));
  secondInventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));

  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true),
    new PlayerInput(
      secondPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));

  ProjectileReplicationSnapshot[] sentries = simulation.CreateProjectileReplicationSnapshots()
    .Where(projectile => projectile.IsActive && projectile.IsSentry)
    .ToArray();
  if (sentries.Length != 2 || !firstInventory.GetSlot(0).IsEmpty ||
      !secondInventory.GetSlot(0).IsEmpty ||
      simulation.CreateItemUsedEvents().Count != 2)
  {
    throw new InvalidOperationException(
      "Non-overlapping sentry placements were not admitted independently.");
  }

  using DomeSimulation ordinarySimulation = new(new WorldGrid(400, 300));
  PlayerHandle sentryPlayer = ordinarySimulation.CreatePlayer(
    new SimulationVector(34.0f, 0.0f));
  InventoryComponent sentryInventory = ordinarySimulation.GetInventory(sentryPlayer);
  sentryInventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));
  ordinarySimulation.Tick(new SimulationInputBatch(new PlayerInput(
    sentryPlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));

  PlayerHandle ordinaryPlayer = ordinarySimulation.CreatePlayer(
    new SimulationVector(34.0f, 0.0f));
  InventoryComponent ordinaryInventory = ordinarySimulation.GetInventory(ordinaryPlayer);
  ordinaryInventory.SetSlot(0, new ItemStack(8, 1));
  ordinarySimulation.Tick(new SimulationInputBatch(new PlayerInput(
    ordinaryPlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  ProjectileReplicationSnapshot ordinaryProjectile = ordinarySimulation
    .CreateProjectileReplicationSnapshots()
    .Single(projectile => projectile.Owner == ordinaryPlayer && projectile.IsActive);
  if (ordinaryProjectile.IsSentry ||
      ordinaryInventory.GetSlot(0) != new ItemStack(8, 1) ||
      ordinarySimulation.CreateItemUsedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "An ordinary projectile use was incorrectly blocked by an active sentry.");
  }

  Console.WriteLine("PASS: non-overlapping sentries and ordinary projectiles remain admitted");
}

static void VerifyExpiredSentryDoesNotBlockPlacement()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(42.0f, 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    player,
    42.0f,
    0.75f,
    1,
    Damage: 14,
    LifetimeTicks: 30,
    ProjectileType: 2,
    ProjectileSpeed: 5.0f,
    IsSentry: true));
  simulation.Tick(new SimulationInputBatch());
  Arch.Core.QueryDescription expiredSentryQuery = new Arch.Core.QueryDescription()
    .WithAll<ProjectileSentryComponent, ProjectileLifetimeComponent>();
  simulation.World.Query(
    in expiredSentryQuery,
    (Arch.Core.Entity entity, ref ProjectileLifetimeComponent lifetime) =>
    {
      lifetime.RemainingTicks = 1;
    });

  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemSentryType, 1));
  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  ProjectileReplicationSnapshot[] activeSentries = simulation.CreateProjectileReplicationSnapshots()
    .Where(projectile => projectile.IsActive && projectile.IsSentry)
    .ToArray();
  if (activeSentries.Length != 1 || !inventory.GetSlot(0).IsEmpty ||
      simulation.CreateItemUsedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      $"An expired sentry continued to block a new placement " +
      $"(active={activeSentries.Length}, stack={inventory.GetSlot(0)}, " +
      $"events={simulation.CreateItemUsedEvents().Count}, " +
      $"revision={simulation.CreateItemUseSnapshot(player).State.UseRevision}).");
  }

  Console.WriteLine("PASS: expired sentries do not block placement admission");
}

static void VerifyItemSentryRuntimeLimit()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(50.0f, 50.0f));
  PlayerHandle otherOwner = simulation.CreatePlayer(new SimulationVector(80.0f, 50.0f));

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    50.0f,
    50.0f,
    1,
    Damage: 14,
    LifetimeTicks: 30,
    ProjectileType: 2,
    ProjectileSpeed: 0.0f,
    IsSentry: true));
  simulation.Tick(new SimulationInputBatch());
  Arch.Core.QueryDescription firstSentryQuery = new Arch.Core.QueryDescription()
    .WithAll<ProjectileSentryComponent, ProjectileLifetimeComponent>();
  simulation.World.Query(
    in firstSentryQuery,
    (Arch.Core.Entity entity, ref ProjectileLifetimeComponent lifetime) =>
    {
      lifetime.RemainingTicks = 10;
    });

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    55.0f,
    50.0f,
    1,
    Damage: 14,
    LifetimeTicks: 30,
    ProjectileType: 2,
    ProjectileSpeed: 0.0f,
    IsSentry: true));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    60.0f,
    50.0f,
    1,
    Damage: 14,
    LifetimeTicks: 30,
    ProjectileType: 1,
    ProjectileSpeed: 0.0f));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    otherOwner,
    80.0f,
    50.0f,
    1,
    Damage: 14,
    LifetimeTicks: 30,
    ProjectileType: 2,
    ProjectileSpeed: 0.0f,
    IsSentry: true));

  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<ProjectileReplicationSnapshot> snapshots =
    simulation.CreateProjectileReplicationSnapshots();
  ProjectileReplicationSnapshot[] ownerSentries = snapshots
    .Where(snapshot => snapshot.Owner == owner && snapshot.IsSentry)
    .ToArray();
  ProjectileReplicationSnapshot[] activeOwnerSentries = ownerSentries
    .Where(snapshot => snapshot.IsActive)
    .ToArray();
  ProjectileReplicationSnapshot ordinary = snapshots.Single(
    snapshot => snapshot.Owner == owner && !snapshot.IsSentry);
  ProjectileReplicationSnapshot otherSentry = snapshots.Single(
    snapshot => snapshot.Owner == otherOwner && snapshot.IsSentry && snapshot.IsActive);
  ProjectileReplicationSnapshot tombstone = ownerSentries.Single(snapshot => !snapshot.IsActive);

  if (activeOwnerSentries.Length != 1 || tombstone.TombstoneReason !=
      ProjectileTombstoneReason.Administrative ||
      tombstone.RemainingLifetime >= activeOwnerSentries[0].RemainingLifetime ||
      !ordinary.IsActive || !otherSentry.IsActive)
  {
    throw new InvalidOperationException(
      $"Item sentry runtime limit did not select one same-owner sentry for deferred despawn " +
      $"(ownerActive={activeOwnerSentries.Length}, tombstone={tombstone.TombstoneReason}, " +
      $"tombstoneLifetime={tombstone.RemainingLifetime}, " +
      $"activeLifetime={activeOwnerSentries[0].RemainingLifetime}, " +
      $"ordinary={ordinary.IsActive}, otherOwner={otherSentry.IsActive}).");
  }

  bool persistentDuringEvent = ProjectileTurretPersistencePolicy.CanWipe(
    projectileType: 663,
    owner: owner,
    localPlayer: owner,
    isSentry: true,
    eventActive: true);
  bool persistentOutsideEvent = ProjectileTurretPersistencePolicy.CanWipe(
    projectileType: 663,
    owner: owner,
    localPlayer: owner,
    isSentry: true,
    eventActive: false);
  if (persistentDuringEvent || !persistentOutsideEvent)
  {
    throw new InvalidOperationException(
      "Item sentry runtime reused an incorrect DD2 persistence gate.");
  }

  Console.WriteLine(
    "PASS: item sentry runtime limits same-owner sentries with deferred administrative despawn");
}

static void VerifyItemSentryEventActiveRuntimeReconcile()
{
  ProjectileDefinitionRegistry definitions = new([
    new ProjectileDefinition(
      663,
      1,
      10,
      1200,
      new ColliderComponent(0.5f, 0.5f),
      true,
      false,
      1,
      IsSentry: true)]);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), definitions);
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(50.0f, 50.0f));
  if (!simulation.TrySetPlayerMaximumTurrets(owner, 2))
  {
    throw new InvalidOperationException("The sentry maximum-turret authority rejected the owner.");
  }

  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    50.0f,
    50.0f,
    1,
    Damage: 10,
    LifetimeTicks: 1200,
    ProjectileType: 663,
    ProjectileSpeed: 0.0f,
    IsSentry: true));
  simulation.QueueProjectileSpawn(new SpawnProjectileCommand(
    owner,
    60.0f,
    50.0f,
    1,
    Damage: 10,
    LifetimeTicks: 1200,
    ProjectileType: 663,
    ProjectileSpeed: 0.0f,
    IsSentry: true));
  simulation.Tick(new SimulationInputBatch());

  int activeBefore = simulation.CreateProjectileReplicationSnapshots()
    .Count(snapshot => snapshot.Owner == owner && snapshot.IsSentry && snapshot.IsActive);
  if (activeBefore != 2)
  {
    throw new InvalidOperationException(
      $"The event-active sentry fixture did not admit both sentries (active={activeBefore}).");
  }

  if (!simulation.TrySetPlayerMaximumTurrets(owner, 1))
  {
    throw new InvalidOperationException(
      "An event-active sentry reconcile incorrectly queued an administrative wipe.");
  }

  simulation.SetSentryEventActive(true);
  simulation.Tick(new SimulationInputBatch());
  int activeDuringEvent = simulation.CreateProjectileReplicationSnapshots()
    .Count(snapshot => snapshot.Owner == owner && snapshot.IsSentry && snapshot.IsActive);
  if (activeDuringEvent != 2)
  {
    throw new InvalidOperationException(
      $"An event-active sentry reconcile removed a persistent sentry (active={activeDuringEvent}).");
  }

  simulation.SetSentryEventActive(false);
  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<ProjectileReplicationSnapshot> snapshots =
    simulation.CreateProjectileReplicationSnapshots();
  ProjectileReplicationSnapshot[] ownerSentries = snapshots
    .Where(snapshot => snapshot.Owner == owner && snapshot.IsSentry)
    .ToArray();
  if (ownerSentries.Count(snapshot => snapshot.IsActive) != 1 ||
      ownerSentries.Count(snapshot => !snapshot.IsActive) != 1 ||
      ownerSentries.Single(snapshot => !snapshot.IsActive).TombstoneReason !=
        ProjectileTombstoneReason.Administrative)
  {
    throw new InvalidOperationException(
      "An event-inactive sentry reconcile did not commit one administrative tombstone.");
  }

  Console.WriteLine(
    "PASS: event-active sentry reconciliation preserves persistent projectiles until event end");
}

static void VerifyItemSentryPlayerAuthority()
{
  if (typeof(PlayerSentryStateComponent).GetField("MaximumTurrets") is not null ||
      typeof(PlayerSentryStateComponent).GetProperty("MaximumTurrets")?.SetMethod?.IsPublic == true)
  {
    throw new InvalidOperationException(
      "Player sentry capacity is directly mutable instead of being owned by its authority system.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle owner = simulation.CreatePlayer(new SimulationVector(50.0f, 50.0f));
  if (!simulation.TryGetPlayerMaximumTurrets(owner, out int initialMaximum) ||
      initialMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets)
  {
    throw new InvalidOperationException(
      $"The player sentry authority did not expose its default capacity " +
      $"(maximum={initialMaximum}).");
  }

  int updatedMaximum = 0;
  if (!simulation.TrySetPlayerMaximumTurrets(owner, 3) ||
      !simulation.TryGetPlayerMaximumTurrets(owner, out updatedMaximum) ||
      updatedMaximum != 3)
  {
    throw new InvalidOperationException(
      $"The player sentry authority did not publish an accepted capacity " +
      $"(maximum={updatedMaximum}).");
  }

  try
  {
    simulation.TrySetPlayerMaximumTurrets(owner, -1);
    throw new InvalidOperationException(
      "The player sentry authority accepted a negative capacity.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  int retainedMaximum = 0;
  if (!simulation.TryGetPlayerMaximumTurrets(owner, out retainedMaximum) ||
      retainedMaximum != 3)
  {
    throw new InvalidOperationException(
      $"A rejected player sentry capacity changed authoritative state " +
      $"(maximum={retainedMaximum}).");
  }

  Arch.Core.Entity ownerEntity = FindPlayerEntity(simulation, owner);
  ref PlayerLifecycleComponent ownerLifecycle =
    ref simulation.World.Get<PlayerLifecycleComponent>(ownerEntity);
  ownerLifecycle.IsActive = false;
  if (simulation.TrySetPlayerMaximumTurrets(owner, 4) ||
      simulation.TryGetPlayerMaximumTurrets(owner, out _))
  {
    throw new InvalidOperationException(
      "The player sentry authority accepted an inactive player entity.");
  }

  if (simulation.TryGetPlayerMaximumTurrets(new PlayerHandle(999), out _))
  {
    throw new InvalidOperationException(
      "The player sentry authority exposed capacity for an unknown player.");
  }

  using Arch.Core.World forgedWorld = Arch.Core.World.Create();
  Arch.Core.Entity forgedEntity = forgedWorld.Create(new PlayerSentryStateComponent());
  Dictionary<PlayerHandle, Arch.Core.Entity> forgedPlayers = new()
  {
    [new PlayerHandle(1)] = forgedEntity
  };
  if (new PlayerSentryAuthoritySystem().TryGetMaximumTurrets(
        forgedWorld,
        forgedPlayers,
        new PlayerHandle(1),
        out _))
  {
    throw new InvalidOperationException(
      "The player sentry authority accepted an entity without player identity ownership.");
  }

  Console.WriteLine(
    "PASS: player sentry capacity is exposed through a server-owned authority boundary");
}

static void VerifyItemSentryEquipmentBonus()
{
  LegacyItemDefinitionRecord legacyDefinition = new(
    ItemType: 15,
    MaxStack: 1,
    EquipmentSlot: ItemEquipmentSlot.Accessory,
    Accessory: true,
    SentryCapacityBonus: 2);
  ItemDefinition adaptedDefinition = LegacyItemDefinitionAdapter.ToDefinition(legacyDefinition);
  if (adaptedDefinition.Equipment is not ItemEquipmentDefinition equipmentDefinition ||
      equipmentDefinition.SentryCapacityBonus != 2)
  {
    throw new InvalidOperationException(
      "The legacy adapter did not preserve the sentry capacity equipment bonus.");
  }

  try
  {
    _ = LegacyItemDefinitionAdapter.ToDefinition(
      new LegacyItemDefinitionRecord(
        ItemType: 17,
        MaxStack: 1,
        SentryCapacityBonus: 1));
    throw new InvalidOperationException(
      "The legacy adapter accepted sentry capacity metadata without an equipment slot.");
  }
  catch (ArgumentException)
  {
  }

  try
  {
    _ = LegacyItemDefinitionAdapter.ToDefinition(
      new LegacyItemDefinitionRecord(
        ItemType: 16,
        MaxStack: 1,
        EquipmentSlot: ItemEquipmentSlot.Accessory,
        Accessory: true,
        SentryCapacityBonus: -1));
    throw new InvalidOperationException(
      "The legacy adapter accepted a negative sentry capacity equipment bonus.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  ItemDefinitionRegistry definitions = new([adaptedDefinition]);
  InventoryComponent inventory = new();
  inventory.SetSlot(0, new ItemStack(adaptedDefinition.ItemType, 1));
  EquipmentStateCollectionComponent equipmentStates = new();
  equipmentStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Accessory,
    0,
    IsVanity: false));
  int bonus = new PlayerSentryEquipmentSystem().CalculateCapacityBonus(
    equipmentStates,
    inventory,
    definitions);
  if (bonus != 2)
  {
    throw new InvalidOperationException(
      $"A valid non-vanity sentry equipment state produced the wrong bonus (bonus={bonus}).");
  }

  equipmentStates.Remove(ItemEquipmentSlot.Accessory);
  equipmentStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Accessory,
    0,
    IsVanity: true));
  if (new PlayerSentryEquipmentSystem().CalculateCapacityBonus(
        equipmentStates,
        inventory,
        definitions) != 0)
  {
    throw new InvalidOperationException(
      "A vanity equipment state contributed to sentry capacity.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  InventoryComponent simulationInventory = simulation.GetInventory(player);
  simulationInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemSentryEquipmentType, 1));
  simulation.QueueEquipItem(player, 0, isVanity: false);
  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryGetPlayerMaximumTurrets(player, out int equippedMaximum) ||
      equippedMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 1)
  {
    throw new InvalidOperationException(
      $"Equipping a sentry bonus item did not update player capacity (maximum={equippedMaximum}).");
  }

  simulation.QueueUnequipItem(player, ItemEquipmentSlot.Accessory);
  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryGetPlayerMaximumTurrets(player, out int unequippedMaximum) ||
      unequippedMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets)
  {
    throw new InvalidOperationException(
      $"Unequipping a sentry bonus item did not restore player capacity " +
      $"(maximum={unequippedMaximum}).");
  }

  Console.WriteLine(
    "PASS: sentry equipment metadata flows through player authority into runtime capacity");
}

static void VerifyLegacySentryEquipmentIdMapping()
{
  (ushort itemType, int capacityBonus)[] expectedMappings =
  [
    (3797, 1),
    (3800, 1),
    (3803, 1),
    (3806, 1),
    (3871, 2),
    (3874, 2),
    (3877, 2),
    (3880, 2),
    (3381, 1)
  ];
  if (LegacySentryEquipmentRegistry.Definitions.Count != expectedMappings.Length)
  {
    throw new InvalidOperationException(
      "The legacy sentry equipment registry did not preserve the exact source mapping count.");
  }

  for (int index = 0; index < expectedMappings.Length; index++)
  {
    (ushort itemType, int expectedBonus) = expectedMappings[index];
    if (!LegacySentryEquipmentRegistry.TryGet(
          itemType,
          out ItemEquipmentDefinition registryDefinition) ||
        registryDefinition.Slot != ItemEquipmentSlot.Head ||
        registryDefinition.SentryCapacityBonus != expectedBonus)
    {
      throw new InvalidOperationException(
        $"Legacy sentry equipment mapping for item {itemType} was not source-backed.");
    }

    ItemDefinition adaptedDefinition = LegacyItemDefinitionAdapter.ToDefinition(
      new LegacyItemDefinitionRecord(itemType, MaxStack: 1));
    if (adaptedDefinition.Equipment is not ItemEquipmentDefinition equipmentDefinition ||
        equipmentDefinition.Slot != ItemEquipmentSlot.Head ||
        equipmentDefinition.SentryCapacityBonus != expectedBonus)
    {
      throw new InvalidOperationException(
        $"The legacy adapter did not project sentry equipment item {itemType} " +
        "into a Head definition.");
    }
  }

  if (LegacySentryEquipmentRegistry.TryGet(3798, out _) ||
      LegacySentryEquipmentRegistry.TryGet(ushort.MaxValue, out _))
  {
    throw new InvalidOperationException(
      "The legacy sentry equipment registry classified an unmapped item as a sentry bonus item.");
  }

  using DomeSimulation registrySimulation = new(new WorldGrid(400, 300));
  for (int index = 0; index < expectedMappings.Length; index++)
  {
    (ushort itemType, int expectedBonus) = expectedMappings[index];
    if (!registrySimulation.ItemDefinitions.TryGet(
          itemType,
          out ItemDefinition simulationDefinition) ||
        simulationDefinition.Equipment is not ItemEquipmentDefinition equipmentDefinition ||
        equipmentDefinition.Slot != ItemEquipmentSlot.Head ||
        equipmentDefinition.SentryCapacityBonus != expectedBonus)
    {
      throw new InvalidOperationException(
        $"DomeSimulation did not build the default definition for sentry item {itemType} " +
        "from the source registry.");
    }
  }

  try
  {
    _ = LegacyItemDefinitionAdapter.ToDefinition(
      new LegacyItemDefinitionRecord(
        ItemType: 3797,
        MaxStack: 1,
        SentryCapacityBonus: 2));
    throw new InvalidOperationException(
      "The legacy adapter accepted a sentry bonus that conflicts with the source mapping.");
  }
  catch (ArgumentException)
  {
  }

  using DomeSimulation plusOneSimulation = new(new WorldGrid(400, 300));
  PlayerHandle plusOnePlayer = plusOneSimulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  InventoryComponent plusOneInventory = plusOneSimulation.GetInventory(plusOnePlayer);
  plusOneInventory.SetSlot(0, new ItemStack(3797, 1));
  plusOneSimulation.QueueEquipItem(plusOnePlayer, 0, isVanity: false);
  plusOneSimulation.Tick(new SimulationInputBatch());
  if (!plusOneSimulation.TryGetPlayerMaximumTurrets(plusOnePlayer, out int plusOneMaximum) ||
      plusOneMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 1)
  {
    throw new InvalidOperationException(
      $"The legacy +1 sentry armor mapping did not reach player capacity " +
      $"(maximum={plusOneMaximum}).");
  }

  using DomeSimulation plusTwoSimulation = new(new WorldGrid(400, 300));
  PlayerHandle plusTwoPlayer = plusTwoSimulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  InventoryComponent plusTwoInventory = plusTwoSimulation.GetInventory(plusTwoPlayer);
  plusTwoInventory.SetSlot(0, new ItemStack(3871, 1));
  plusTwoSimulation.QueueEquipItem(plusTwoPlayer, 0, isVanity: false);
  plusTwoSimulation.Tick(new SimulationInputBatch());
  if (!plusTwoSimulation.TryGetPlayerMaximumTurrets(plusTwoPlayer, out int plusTwoMaximum) ||
      plusTwoMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 2)
  {
    throw new InvalidOperationException(
      $"The legacy +2 sentry armor mapping did not reach player capacity " +
      $"(maximum={plusTwoMaximum}).");
  }

  Console.WriteLine(
    "PASS: legacy sentry armor IDs map through definitions into player turret capacity");
}

static void VerifyLegacySentryBuff348Capacity()
{
  const ushort warTableBuffType = 348;
  if (LegacySentryBuffRegistry.CapacityBonuses.Count != 1 ||
      !LegacySentryBuffRegistry.TryGet(warTableBuffType, out int warTableBonus) ||
      warTableBonus != 1)
  {
    throw new InvalidOperationException(
      "The legacy sentry buff registry did not preserve War Table buff 348 as a +1 bonus.");
  }

  if (LegacySentryBuffRegistry.TryGet(347, out _) ||
      LegacySentryBuffRegistry.TryGet(349, out _) ||
      LegacySentryBuffRegistry.TryGet(ushort.MaxValue, out _))
  {
    throw new InvalidOperationException(
      "The legacy sentry buff registry classified an unmapped buff as a sentry bonus.");
  }

  BuffCollectionComponent activeBuffs = new();
  PlayerHandle source = new(1);
  activeBuffs.Add(warTableBuffType, 20, source);
  activeBuffs.Add(347, 20, source);
  activeBuffs.Add(349, 20, source);
  activeBuffs.Add(ushort.MaxValue, 20, source);
  PlayerSentryBuffSystem buffSystem = new();
  if (buffSystem.CalculateCapacityBonus(activeBuffs) != 1)
  {
    throw new InvalidOperationException(
      "An active War Table buff did not contribute exactly one sentry capacity slot.");
  }

  activeBuffs.SetAt(0, new BuffEntry(warTableBuffType, RemainingTicks: 0, source));
  if (buffSystem.CalculateCapacityBonus(activeBuffs) != 0)
  {
    throw new InvalidOperationException(
      "An expired War Table buff continued to contribute sentry capacity.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  BuffCollectionComponent buffs = GetPlayerBuffCollection(simulation, player);
  buffs.Add(warTableBuffType, 2, player);
  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryGetPlayerMaximumTurrets(player, out int activeMaximum) ||
      activeMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 1)
  {
    throw new InvalidOperationException(
      $"An active War Table buff did not update authoritative capacity (maximum={activeMaximum}).");
  }

  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryGetPlayerMaximumTurrets(player, out int expiredMaximum) ||
      expiredMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets)
  {
    throw new InvalidOperationException(
      $"An expired War Table buff did not restore authoritative capacity " +
      $"(maximum={expiredMaximum}).");
  }

  using DomeSimulation combinedSimulation = new(new WorldGrid(400, 300));
  PlayerHandle combinedPlayer = combinedSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  InventoryComponent combinedInventory = combinedSimulation.GetInventory(combinedPlayer);
  combinedInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemSentryEquipmentType, 1));
  GetPlayerBuffCollection(combinedSimulation, combinedPlayer).Add(
    warTableBuffType,
    3,
    combinedPlayer);
  combinedSimulation.QueueEquipItem(combinedPlayer, 0, isVanity: false);
  combinedSimulation.Tick(new SimulationInputBatch());
  if (!combinedSimulation.TryGetPlayerMaximumTurrets(combinedPlayer, out int combinedMaximum) ||
      combinedMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 2)
  {
    throw new InvalidOperationException(
      $"Equipment and War Table capacity bonuses did not add together " +
      $"(maximum={combinedMaximum}).");
  }

  combinedSimulation.QueueUnequipItem(combinedPlayer, ItemEquipmentSlot.Accessory);
  combinedSimulation.Tick(new SimulationInputBatch());
  if (!combinedSimulation.TryGetPlayerMaximumTurrets(combinedPlayer, out int buffOnlyMaximum) ||
      buffOnlyMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 1)
  {
    throw new InvalidOperationException(
      $"Removing equipment incorrectly removed the active War Table bonus " +
      $"(maximum={buffOnlyMaximum}).");
  }

  combinedSimulation.Tick(new SimulationInputBatch());
  if (!combinedSimulation.TryGetPlayerMaximumTurrets(combinedPlayer, out int restoredMaximum) ||
      restoredMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets)
  {
    throw new InvalidOperationException(
      $"An expired War Table bonus did not restore capacity after equipment removal " +
      $"(maximum={restoredMaximum}).");
  }

  Console.WriteLine(
    "PASS: legacy War Table buff 348 flows through player authority into sentry capacity");
}

static void VerifyLegacySentryArmorSetCapacity()
{
  (ushort HeadItemType, ushort BodyItemType, ushort LegItemType, LegacySentryArmorSetClass SetClass,
    LegacySentryArmorSetTier Tier)[] expectedSets =
  [
    (3800, 3801, 3802, LegacySentryArmorSetClass.Squire, LegacySentryArmorSetTier.Tier2),
    (3797, 3798, 3799, LegacySentryArmorSetClass.Apprentice, LegacySentryArmorSetTier.Tier2),
    (3803, 3804, 3805, LegacySentryArmorSetClass.Huntress, LegacySentryArmorSetTier.Tier2),
    (3806, 3807, 3808, LegacySentryArmorSetClass.Monk, LegacySentryArmorSetTier.Tier2),
    (3871, 3872, 3873, LegacySentryArmorSetClass.Squire, LegacySentryArmorSetTier.Tier3),
    (3874, 3875, 3876, LegacySentryArmorSetClass.Apprentice, LegacySentryArmorSetTier.Tier3),
    (3877, 3878, 3879, LegacySentryArmorSetClass.Huntress, LegacySentryArmorSetTier.Tier3),
    (3880, 3881, 3882, LegacySentryArmorSetClass.Monk, LegacySentryArmorSetTier.Tier3)
  ];
  if (LegacySentryArmorSetRegistry.Definitions.Count != expectedSets.Length)
  {
    throw new InvalidOperationException(
      "The legacy sentry armor-set registry did not preserve the eight source branches.");
  }

  for (int index = 0; index < expectedSets.Length; index++)
  {
    (ushort headItemType, ushort bodyItemType, ushort legItemType,
      LegacySentryArmorSetClass setClass,
      LegacySentryArmorSetTier tier) = expectedSets[index];
    if (!LegacySentryArmorSetRegistry.TryGet(
          headItemType,
          bodyItemType,
          legItemType,
          out LegacySentryArmorSetDefinition definition) ||
        definition.SetClass != setClass || definition.Tier != tier ||
        definition.CapacityBonus != 1 || definition.HeadItemType != headItemType ||
        definition.BodyItemType != bodyItemType || definition.LegItemType != legItemType)
    {
      throw new InvalidOperationException(
        $"Legacy sentry armor-set branch {headItemType}/{bodyItemType}/{legItemType} " +
        "was not source-backed.");
    }
  }

  if (LegacySentryArmorSetRegistry.TryGet(3803, 3804, 3802, out _) ||
      LegacySentryArmorSetRegistry.TryGet(3800, 3801, 3805, out _) ||
      LegacySentryArmorSetRegistry.TryGet(3800, 3804, 3802, out _) ||
      LegacySentryArmorSetRegistry.TryGet(ushort.MaxValue, 3801, 3802, out _))
  {
    throw new InvalidOperationException(
      "The sentry armor-set registry accepted an incomplete or non-source combination.");
  }

  using DomeSimulation tierTwoSimulation = new(new WorldGrid(400, 300));
  PlayerHandle tierTwoPlayer = tierTwoSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  InventoryComponent tierTwoInventory = tierTwoSimulation.GetInventory(tierTwoPlayer);
  tierTwoInventory.SetSlot(0, new ItemStack(3797, 1));
  tierTwoInventory.SetSlot(1, new ItemStack(3798, 1));
  tierTwoInventory.SetSlot(2, new ItemStack(3799, 1));
  tierTwoSimulation.QueueEquipItem(tierTwoPlayer, 0, isVanity: false);
  tierTwoSimulation.QueueEquipItem(tierTwoPlayer, 1, isVanity: false);
  tierTwoSimulation.QueueEquipItem(tierTwoPlayer, 2, isVanity: false);
  tierTwoSimulation.Tick(new SimulationInputBatch());
  if (!tierTwoSimulation.TryGetPlayerMaximumTurrets(tierTwoPlayer, out int tierTwoMaximum) ||
      tierTwoMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 2)
  {
    throw new InvalidOperationException(
      $"A complete Tier-2 sentry armor set did not add one set slot " +
      $"in addition to its head bonus (maximum={tierTwoMaximum}).");
  }

  tierTwoSimulation.QueueUnequipItem(tierTwoPlayer, ItemEquipmentSlot.Legs);
  tierTwoSimulation.Tick(new SimulationInputBatch());
  if (!tierTwoSimulation.TryGetPlayerMaximumTurrets(tierTwoPlayer, out int partialMaximum) ||
      partialMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 1)
  {
    throw new InvalidOperationException(
      $"An incomplete sentry armor set retained its set bonus (maximum={partialMaximum}).");
  }

  using DomeSimulation tierThreeSimulation = new(new WorldGrid(400, 300));
  PlayerHandle tierThreePlayer = tierThreeSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  InventoryComponent tierThreeInventory = tierThreeSimulation.GetInventory(tierThreePlayer);
  tierThreeInventory.SetSlot(0, new ItemStack(3871, 1));
  tierThreeInventory.SetSlot(1, new ItemStack(3872, 1));
  tierThreeInventory.SetSlot(2, new ItemStack(3873, 1));
  tierThreeSimulation.QueueEquipItem(tierThreePlayer, 0, isVanity: false);
  tierThreeSimulation.QueueEquipItem(tierThreePlayer, 1, isVanity: false);
  tierThreeSimulation.QueueEquipItem(tierThreePlayer, 2, isVanity: false);
  tierThreeSimulation.Tick(new SimulationInputBatch());
  if (!tierThreeSimulation.TryGetPlayerMaximumTurrets(tierThreePlayer, out int tierThreeMaximum) ||
      tierThreeMaximum != PlayerSentryStateComponent.DefaultMaximumTurrets + 3)
  {
    throw new InvalidOperationException(
      $"A complete Tier-3 sentry armor set double-counted or lost its set slot " +
      $"(maximum={tierThreeMaximum}).");
  }

  InventoryComponent directInventory = new();
  directInventory.SetSlot(0, new ItemStack(3797, 1));
  directInventory.SetSlot(1, new ItemStack(3798, 1));
  directInventory.SetSlot(2, new ItemStack(3799, 1));
  ItemDefinitionRegistry directDefinitions = new([
    new ItemDefinition(
      3797,
      1,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head)),
    new ItemDefinition(
      3798,
      1,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Body)),
    new ItemDefinition(
      3799,
      1,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Legs))]);
  EquipmentStateCollectionComponent directStates = new();
  directStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Head,
    0,
    IsVanity: true));
  directStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Body,
    1,
    IsVanity: false));
  directStates.Add(new Terraria.Dome.Simulation.Items.Components.ItemEquipmentStateComponent(
    ItemEquipmentSlot.Legs,
    2,
    IsVanity: false));
  if (new PlayerSentryArmorSetSystem().CalculateCapacityBonus(
        directStates,
        directInventory,
        directDefinitions) != 0)
  {
    throw new InvalidOperationException(
      "A vanity armor piece contributed to sentry armor-set capacity.");
  }

  Console.WriteLine(
    "PASS: legacy sentry armor-set combinations flow through player authority into capacity");
}

static void VerifyLegacyNoMeleeSleepWake()
{
  ItemDefinition ordinaryDefinition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(
      ItemType: 6001,
      MaxStack: 1,
      UseTime: 2,
      UseAnimation: 2,
      Damage: 10,
      Melee: true));
  if (ordinaryDefinition.Combat is not ItemCombatDefinition ordinaryCombat ||
      ordinaryCombat.Damage != 10 || ordinaryCombat.NoMelee)
  {
    throw new InvalidOperationException(
      "The legacy adapter did not preserve the ordinary melee noMelee=false branch.");
  }

  ItemDefinition noMeleeDefinition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(
      ItemType: 6002,
      MaxStack: 1,
      UseTime: 2,
      UseAnimation: 2,
      Damage: 10,
      NoMelee: true,
      Melee: true));
  if (ordinaryDefinition.IsNoMelee || !noMeleeDefinition.IsNoMelee)
  {
    throw new InvalidOperationException(
      "Item definition did not expose the authoritative noMelee query.");
  }
  if (noMeleeDefinition.Combat is not ItemCombatDefinition noMeleeCombat ||
      noMeleeCombat.Damage != 10 || !noMeleeCombat.NoMelee)
  {
    throw new InvalidOperationException(
      "The legacy adapter did not preserve the source noMelee=true combat flag.");
  }

  PlayerSleepWakeInput wakeInput = new(
    IsSleeping: true,
    ItemAnimationTicks: 1,
    ItemDamage: 10,
    NoMelee: false);
  if (!PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput) ||
      PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput with { NoMelee = true }) ||
      PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput with { IsSleeping = false }) ||
      PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput with { ItemAnimationTicks = 0 }) ||
      PlayerSleepWakePolicy.ShouldWakeForItemUse(wakeInput with { ItemDamage = 0 }))
  {
    throw new InvalidOperationException(
      "The noMelee sleep-wake policy did not preserve the legacy item animation guards.");
  }

  using DomeSimulation noMeleeSimulation = new(new WorldGrid(400, 300));
  PlayerHandle noMeleePlayer = noMeleeSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  InventoryComponent noMeleeInventory = noMeleeSimulation.GetInventory(noMeleePlayer);
  noMeleeInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemNoMeleeType, 1));
  if (!noMeleeSimulation.TrySetPlayerSleeping(noMeleePlayer, true) ||
      !noMeleeSimulation.CreatePlayerStateSnapshot(noMeleePlayer).IsSleeping)
  {
    throw new InvalidOperationException(
      "The player sleep authority did not accept an active player sleep request.");
  }

  noMeleeSimulation.Tick(new SimulationInputBatch(new PlayerInput(
    noMeleePlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  noMeleeSimulation.Tick(new SimulationInputBatch());
  PlayerStateSnapshot noMeleeState = noMeleeSimulation.CreatePlayerStateSnapshot(noMeleePlayer);
  if (!noMeleeState.IsSleeping || noMeleeState.SleepWakeReason != PlayerSleepWakeReason.None)
  {
    throw new InvalidOperationException(
      "A noMelee item incorrectly woke a sleeping player during its item animation.");
  }

  Arch.Core.Entity noMeleeEntity = FindPlayerEntity(noMeleeSimulation, noMeleePlayer);
  ref PlayerLifecycleComponent noMeleeLifecycle =
    ref noMeleeSimulation.World.Get<PlayerLifecycleComponent>(noMeleeEntity);
  noMeleeLifecycle.IsActive = false;
  if (noMeleeSimulation.TrySetPlayerSleeping(noMeleePlayer, false) ||
      noMeleeSimulation.TryGetPlayerSleepState(noMeleePlayer, out _) ||
      noMeleeSimulation.TryGetPlayerSleepState(new PlayerHandle(6003), out _))
  {
    throw new InvalidOperationException(
      "Sleep authority accepted an inactive or unknown player mapping.");
  }
  noMeleeLifecycle.IsActive = true;

  using DomeSimulation ordinarySimulation = new(new WorldGrid(400, 300));
  PlayerHandle ordinaryPlayer = ordinarySimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  InventoryComponent ordinaryInventory = ordinarySimulation.GetInventory(ordinaryPlayer);
  ordinaryInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemMeleeUseType, 1));
  if (!ordinarySimulation.TrySetPlayerSleeping(ordinaryPlayer, true))
  {
    throw new InvalidOperationException(
      "The ordinary melee sleep fixture could not enter authoritative sleep.");
  }

  ordinarySimulation.Tick(new SimulationInputBatch(new PlayerInput(
    ordinaryPlayer,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  ordinarySimulation.Tick(new SimulationInputBatch());
  PlayerStateSnapshot ordinaryState = ordinarySimulation.CreatePlayerStateSnapshot(ordinaryPlayer);
  if (ordinaryState.IsSleeping ||
      ordinaryState.SleepWakeReason != PlayerSleepWakeReason.MeleeItem)
  {
    throw new InvalidOperationException(
      "An ordinary damaging item did not wake a sleeping player during its item animation.");
  }

  using DomeSimulation singleAnimationSimulation = new(new WorldGrid(400, 300));
  PlayerHandle singleAnimationPlayer = singleAnimationSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  singleAnimationSimulation.GetInventory(singleAnimationPlayer).SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemMeleeUseType, 1));
  if (!singleAnimationSimulation.TrySetPlayerSleeping(singleAnimationPlayer, true))
  {
    throw new InvalidOperationException(
      "The single-tick melee fixture could not enter authoritative sleep.");
  }

  Arch.Core.Entity singleAnimationEntity =
    FindPlayerEntity(singleAnimationSimulation, singleAnimationPlayer);
  ref Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent singleAnimationUse =
    ref singleAnimationSimulation.World.Get<
      Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent>(singleAnimationEntity);
  singleAnimationUse.AnimationTicks = 1;
  singleAnimationSimulation.Tick(new SimulationInputBatch());
  PlayerStateSnapshot singleAnimationState =
    singleAnimationSimulation.CreatePlayerStateSnapshot(singleAnimationPlayer);
  if (singleAnimationState.IsSleeping ||
      singleAnimationState.SleepWakeReason != PlayerSleepWakeReason.MeleeItem)
  {
    throw new InvalidOperationException(
      "A one-tick item animation was missed by the sleep wake system.");
  }

  Console.WriteLine(
    "PASS: legacy noMelee metadata flows through item definitions into sleep wake authority");
}

static void VerifyLegacyFishingPoleBobberAdmission()
{
  ItemDefinition fishingPoleDefinition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(
      ItemType: 6003,
      MaxStack: 1,
      UseTime: 1,
      UseAnimation: 1,
      ShootType: DomeSimulation.FixtureFishingBobberProjectileType,
      ShootSpeed: 6.0f,
      FishingPolePower: 30));
  if (fishingPoleDefinition.Gathering?.FishingPolePower != 30 ||
      fishingPoleDefinition.Use?.ShootType != DomeSimulation.FixtureFishingBobberProjectileType)
  {
    throw new InvalidOperationException(
      "The legacy adapter did not preserve the fishing-pole and bobber metadata.");
  }

  PlayerFishingUseInput noBobberInput = new(30, HasActiveOwnedBobber: false);
  if (!PlayerFishingUsePolicy.CanUseFishingPole(noBobberInput) ||
      PlayerFishingUsePolicy.CanUseFishingPole(
        noBobberInput with { HasActiveOwnedBobber = true }) ||
      !PlayerFishingUsePolicy.CanUseFishingPole(
        noBobberInput with { FishingPolePower = 0, HasActiveOwnedBobber = true }))
  {
    throw new InvalidOperationException(
      "The fishing-pole policy did not preserve the source bobber admission guards.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle firstPlayer = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  PlayerHandle secondPlayer = simulation.CreatePlayer(new SimulationVector(80.0f, 0.0f));
  InventoryComponent firstInventory = simulation.GetInventory(firstPlayer);
  InventoryComponent secondInventory = simulation.GetInventory(secondPlayer);
  firstInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemFishingPoleType, 1));
  secondInventory.SetSlot(
    0,
    new ItemStack(DomeSimulation.FixtureItemFishingPoleType, 1));

  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));
  if (simulation.CreateProjectileReplicationSnapshots()
        .Count(projectile => projectile.Owner == firstPlayer && projectile.IsActive &&
          projectile.IsBobber) != 1)
  {
    throw new InvalidOperationException(
      "A fishing-pole use did not create one authoritative bobber.");
  }

  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      secondPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));
  if (simulation.CreateProjectileReplicationSnapshots()
        .Count(projectile => projectile.Owner == secondPlayer && projectile.IsActive &&
          projectile.IsBobber) != 1)
  {
    throw new InvalidOperationException(
      "A bobber owned by another player incorrectly blocked fishing-pole use.");
  }

  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));
  if (simulation.CreateProjectileReplicationSnapshots()
        .Count(projectile => projectile.Owner == firstPlayer && projectile.IsActive &&
          projectile.IsBobber) != 1 ||
      simulation.CreateItemUsedEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "A player with an active bobber was allowed to start a duplicate fishing use.");
  }

  Arch.Core.QueryDescription bobberLifetimeQuery = new Arch.Core.QueryDescription()
    .WithAll<ProjectileBobberComponent, ProjectileOwnerComponent, ProjectileLifetimeComponent>();
  bool expiredBobberFound = false;
  simulation.World.Query(
    in bobberLifetimeQuery,
    (Arch.Core.Entity entity, ref ProjectileOwnerComponent owner,
      ref ProjectileLifetimeComponent lifetime) =>
    {
      if (owner.Owner == firstPlayer)
      {
        lifetime.RemainingTicks = 0;
        expiredBobberFound = true;
      }
    });
  if (!expiredBobberFound)
  {
    throw new InvalidOperationException("The first player bobber was not available for expiry testing.");
  }

  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 0,
      UseItem: true)));
  if (simulation.CreateProjectileReplicationSnapshots()
        .Count(projectile => projectile.Owner == firstPlayer && projectile.IsActive &&
          projectile.IsBobber) != 1 ||
      simulation.CreateItemUsedEvents().Count != 1)
  {
    throw new InvalidOperationException(
      "An expired bobber incorrectly blocked a new fishing-pole use.");
  }

  firstInventory.SetSlot(1, new ItemStack(8, 1));
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch(
    new PlayerInput(
      firstPlayer,
      MoveLeft: false,
      MoveRight: false,
      Jump: false,
      Fire: false,
      SelectedSlot: 1,
      UseItem: true)));
  if (simulation.CreateProjectileReplicationSnapshots()
        .Count(projectile => projectile.Owner == firstPlayer && projectile.IsActive &&
          !projectile.IsBobber) == 0)
  {
    throw new InvalidOperationException(
      "A non-fishing-pole item was incorrectly blocked by an active bobber.");
  }

  Console.WriteLine(
    "PASS: fishingPole metadata enforces one active bobber per player without cross-owner blocking");
}

static void VerifyLegacyDd2SummonSentryAuthority()
{
  ItemDefinition dd2Definition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(
      ItemType: 6004,
      MaxStack: 1,
      UseTime: 1,
      UseAnimation: 1,
      Damage: 17,
      Knockback: 3.0f,
      DamageClass: ItemDamageClass.Summon,
      ShootType: DomeSimulation.FixtureDd2SummonProjectileType,
      ShootSpeed: 1.0f,
      Sentry: true,
      Dd2Summon: true));
  if (!dd2Definition.IsSentry || !dd2Definition.Dd2Summon ||
      dd2Definition.Use?.ShootType != DomeSimulation.FixtureDd2SummonProjectileType)
  {
    throw new InvalidOperationException(
      "The legacy adapter did not preserve the DD2 summon sentry definition.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  if (!simulation.TrySetPlayerMaximumTurrets(player, 0))
  {
    throw new InvalidOperationException(
      "The sentry authority rejected the DD2 event persistence fixture limit.");
  }

  simulation.SetSentryEventActive(true);
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemDd2SummonType, 1));
  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));

  ProjectileReplicationSnapshot[] eventSentries = simulation.CreateProjectileReplicationSnapshots()
    .Where(snapshot => snapshot.Owner == player && snapshot.IsSentry)
    .ToArray();
  Arch.Core.QueryDescription dd2Query = new Arch.Core.QueryDescription()
    .WithAll<ProjectileDd2SummonComponent, ProjectileSentryComponent, ProjectileOwnerComponent>();
  bool hasDd2Component = false;
  simulation.World.Query(
    in dd2Query,
    (Arch.Core.Entity entity, ref ProjectileOwnerComponent owner) =>
    {
      if (owner.Owner == player)
      {
        hasDd2Component = true;
      }
    });
  if (eventSentries.Length != 1 || !eventSentries[0].IsActive || !hasDd2Component)
  {
    throw new InvalidOperationException(
      "An active DD2 event incorrectly cleaned up a source-marked summon sentry.");
  }

  simulation.SetSentryEventActive(false);
  simulation.Tick(new SimulationInputBatch());
  ProjectileReplicationSnapshot[] afterEvent = simulation.CreateProjectileReplicationSnapshots()
    .Where(snapshot => snapshot.Owner == player && snapshot.IsSentry)
    .ToArray();
  if (afterEvent.Count(snapshot => snapshot.IsActive) != 0 ||
      afterEvent.Count(snapshot => !snapshot.IsActive &&
        snapshot.TombstoneReason == ProjectileTombstoneReason.Administrative) != 1)
  {
    throw new InvalidOperationException(
      "An event-inactive DD2 summon sentry did not follow the administrative sentry limit gate.");
  }

  Console.WriteLine(
    "PASS: DD2 summon metadata reaches sentry runtime and event-active persistence authority");
}

static void VerifyShopPurchaseTransaction()
{
  ItemDefinitionRegistry definitions = new([
    new ItemDefinition(71, 9999),
    new ItemDefinition(72, 9999),
    new ItemDefinition(74, 9999),
    new ItemDefinition(200, 10)]);
  ShopOfferDefinition offer = new(7, 200, 1, 100, BuyOnce: true);
  ShopOfferDefinition largeChangeOffer = new(11, 200, 1, 1);
  ShopOfferDefinition unknownItemOffer = new(8, 201, 1, 1);
  ShopOfferDefinition forgedCoinsOffer = new(9, 200, 1, 10000);
  ShopOfferDefinition revisionOverflowOffer = new(10, 200, 1, 100);
  Dictionary<int, ShopOfferDefinition> offers = new()
  {
    [offer.OfferId] = offer,
    [largeChangeOffer.OfferId] = largeChangeOffer,
    [unknownItemOffer.OfferId] = unknownItemOffer,
    [forgedCoinsOffer.OfferId] = forgedCoinsOffer,
    [revisionOverflowOffer.OfferId] = revisionOverflowOffer
  };
  ShopPurchaseSystem purchaseSystem = new(offers);
  InventoryComponent inventory = new();
  inventory.SetSlot(0, new ItemStack(72, 1));
  NpcInteractionComponent interaction = new();
  interaction.Apply(
    new NpcInteractionCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      NpcInteractionKind.Shop,
      "shop-session",
      new SimulationVector(0.0f, 0.0f)),
    1);
  ShopPurchaseState state = new();
  ShopPurchaseResult result = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      7,
      "shop-session",
      2),
    interaction,
    offer,
    inventory,
    definitions,
    state);
  if (!result.IsAccepted || !inventory.GetSlot(1).Equals(new ItemStack(200, 1)) ||
      !inventory.GetSlot(0).IsEmpty || !state.IsPurchased(7))
  {
    throw new InvalidOperationException(
      "Shop purchase did not atomically deliver the item and consume coins.");
  }

  InventoryComponent largeChangeInventory = new();
  largeChangeInventory.SetSlot(0, new ItemStack(74, 1));
  ShopPurchaseResult largeChange = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      11,
      "shop-session",
      3),
    interaction,
    largeChangeOffer,
    largeChangeInventory,
    definitions,
    new ShopPurchaseState());
  if (!largeChange.IsAccepted ||
      largeChangeInventory.GetSlot(0) != new ItemStack(73, 99) ||
      largeChangeInventory.GetSlot(1) != new ItemStack(200, 1) ||
      largeChangeInventory.GetSlot(2) != new ItemStack(72, 99) ||
      largeChangeInventory.GetSlot(3) != new ItemStack(71, 99))
  {
    throw new InvalidOperationException(
      "A platinum payment did not use available empty slots for ordinary-currency change.");
  }

  Console.WriteLine(
    "PASS: ordinary-currency change uses available inventory slots beyond payment stacks");

  InventoryComponent unregisteredInventory = new();
  unregisteredInventory.SetSlot(0, new ItemStack(72, 1));
  long unregisteredRevision = unregisteredInventory.Revision;
  ShopPurchaseResult unregistered = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      99,
      "shop-session",
      4),
    interaction,
    new ShopOfferDefinition(99, 200, 1, 1),
    unregisteredInventory,
    definitions,
    new ShopPurchaseState());
  if (unregistered.IsAccepted || unregisteredInventory.Revision != unregisteredRevision ||
      unregisteredInventory.GetSlot(0) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "A structurally valid but unregistered shop offer bypassed the offer catalog.");
  }

  Console.WriteLine("PASS: unregistered shop offers fail closed at the transaction boundary");

  ShopPurchaseResult duplicate = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      7,
      "shop-session",
      5),
    interaction,
    offer,
    inventory,
    definitions,
    state);
  if (duplicate.IsAccepted)
  {
    throw new InvalidOperationException("Buy-once shop offer was purchased twice.");
  }

  ShopPurchaseResult forgedNpc = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(3),
      7,
      "shop-session",
      6),
    interaction,
    offer,
    inventory,
    definitions,
    new ShopPurchaseState());
  if (forgedNpc.IsAccepted)
  {
    throw new InvalidOperationException("Shop purchase accepted a command for another NPC.");
  }

  InventoryComponent unknownItemInventory = new();
  unknownItemInventory.SetSlot(0, new ItemStack(72, 1));
  long unknownItemRevision = unknownItemInventory.Revision;
  ShopPurchaseResult unknownItem = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      8,
      "shop-session",
      7),
    interaction,
    unknownItemOffer,
    unknownItemInventory,
    definitions,
    new ShopPurchaseState());
  if (unknownItem.IsAccepted || unknownItemInventory.Revision != unknownItemRevision ||
      unknownItemInventory.GetSlot(0) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "A shop offer for an unregistered item did not fail closed atomically.");
  }

  InventoryComponent forgedCoinInventory = new();
  forgedCoinInventory.SetSlot(0, new ItemStack(71, 10000));
  long forgedCoinRevision = forgedCoinInventory.Revision;
  ShopPurchaseResult forgedCoins = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      9,
      "shop-session",
      8),
    interaction,
    forgedCoinsOffer,
    forgedCoinInventory,
    definitions,
    new ShopPurchaseState());
  if (forgedCoins.IsAccepted || forgedCoinInventory.Revision != forgedCoinRevision ||
      forgedCoinInventory.GetSlot(0) != new ItemStack(71, 10000))
  {
    throw new InvalidOperationException(
      "A forged over-limit currency stack did not fail closed atomically.");
  }

  InventoryComponent revisionOverflowInventory = new();
  revisionOverflowInventory.SetSlot(0, new ItemStack(72, 1));
  revisionOverflowInventory.SetSlot(1, new ItemStack(200, 1));
  typeof(InventoryComponent).GetProperty(nameof(InventoryComponent.Revision))!
    .SetValue(revisionOverflowInventory, long.MaxValue);
  long revisionOverflowBefore = revisionOverflowInventory.Revision;
  ItemStack revisionOverflowCoinsBefore = revisionOverflowInventory.GetSlot(0);
  ItemStack revisionOverflowItemBefore = revisionOverflowInventory.GetSlot(1);
  ShopPurchaseResult revisionOverflow = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      10,
      "shop-session",
      9),
    interaction,
    revisionOverflowOffer,
    revisionOverflowInventory,
    definitions,
    new ShopPurchaseState());
  if (revisionOverflow.IsAccepted ||
      revisionOverflowInventory.Revision != revisionOverflowBefore ||
      revisionOverflowInventory.GetSlot(0) != revisionOverflowCoinsBefore ||
      revisionOverflowInventory.GetSlot(1) != revisionOverflowItemBefore)
  {
    throw new InvalidOperationException(
      "A shop purchase did not fail closed when inventory revision could not advance.");
  }

  Console.WriteLine("PASS: ordinary-currency shop purchase is session-bound and buy-once atomic");
}

static void VerifyShopPurchaseTickAuthority()
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  NpcHandle npc = simulation.CreateNpc(
    new SimulationVector(50.0f, 0.0f),
    DomeSimulation.FixtureNpcType);
  NpcHandle otherNpc = simulation.CreateNpc(
    new SimulationVector(52.0f, 0.0f),
    DomeSimulation.FixtureNpcType);
  ShopOfferDefinition offer = new(701, 1, 1, 100, BuyOnce: true);
  simulation.RegisterShopOffer(offer);
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(72, 2));
  long revisionBeforeQueue = inventory.Revision;

  simulation.QueueNpcInteraction(new NpcInteractionCommand(
    player,
    npc,
    NpcInteractionKind.Shop,
    "tick-shop-session",
    new SimulationVector(50.0f, 0.0f)));
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    offer.OfferId,
    "tick-shop-session",
    0));
  if (inventory.Revision != revisionBeforeQueue ||
      inventory.GetSlot(0) != new ItemStack(72, 2) ||
      !inventory.GetSlot(1).IsEmpty)
  {
    throw new InvalidOperationException(
      "A queued shop purchase mutated inventory before the simulation tick.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (inventory.GetSlot(0) != new ItemStack(72, 1) ||
      inventory.GetSlot(1) != new ItemStack(1, 1))
  {
    throw new InvalidOperationException(
      "A valid shop purchase did not commit atomically at the simulation tick boundary.");
  }

  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    offer.OfferId,
    "tick-shop-session",
    1));
  long revisionAfterFirstPurchase = inventory.Revision;
  simulation.Tick(new SimulationInputBatch());
  if (inventory.Revision != revisionAfterFirstPurchase ||
      inventory.GetSlot(0) != new ItemStack(72, 1) ||
      inventory.GetSlot(1) != new ItemStack(1, 1))
  {
    throw new InvalidOperationException(
      "A BuyOnce shop offer was consumed more than once through the tick queue.");
  }

  inventory.SetSlot(2, new ItemStack(72, 1));
  long revisionBeforeInvalidSession = inventory.Revision;
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    offer.OfferId,
    "forged-session",
    2));
  simulation.Tick(new SimulationInputBatch());
  if (inventory.Revision != revisionBeforeInvalidSession ||
      inventory.GetSlot(2) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "A forged shop session changed inventory through the tick queue.");
  }

  simulation.QueueNpcInteraction(new NpcInteractionCommand(
    player,
    npc,
    NpcInteractionKind.Shop,
    "second-shop-session",
    new SimulationVector(50.0f, 0.0f)));
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    otherNpc,
    offer.OfferId,
    "second-shop-session",
    3));
  long revisionBeforeForgedNpc = inventory.Revision;
  simulation.Tick(new SimulationInputBatch());
  if (inventory.Revision != revisionBeforeForgedNpc ||
      inventory.GetSlot(2) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "A shop purchase addressed to another NPC changed inventory through the tick queue.");
  }

  Console.WriteLine(
    "PASS: shop purchase commits only after a valid NPC session at the simulation tick boundary");
}

static void VerifyShopCustomPriceOverride()
{
  ItemDefinitionRegistry definitions = new([
    new ItemDefinition(71, 9999),
    new ItemDefinition(72, 9999),
    new ItemDefinition(200, 10)]);
  ShopOfferDefinition customPriceOffer = new(
    801,
    200,
    1,
    100,
    CustomPriceCopper: 25);
  Dictionary<int, ShopOfferDefinition> offers = new()
  {
    [customPriceOffer.OfferId] = customPriceOffer
  };
  ShopPurchaseSystem purchaseSystem = new(offers);
  NpcInteractionComponent interaction = new();
  interaction.Apply(
    new NpcInteractionCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      NpcInteractionKind.Shop,
      "custom-price-session",
      new SimulationVector(0.0f, 0.0f)),
    1);
  InventoryComponent inventory = new();
  inventory.SetSlot(0, new ItemStack(72, 1));
  ShopPurchaseResult result = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customPriceOffer.OfferId,
      "custom-price-session",
      1),
    interaction,
    customPriceOffer,
    inventory,
    definitions,
    new ShopPurchaseState());
  if (!result.IsAccepted || inventory.GetSlot(0) != new ItemStack(71, 75) ||
      inventory.GetSlot(1) != new ItemStack(200, 1))
  {
    throw new InvalidOperationException(
      "A custom shop price did not override the base ordinary-currency offer price.");
  }

  ShopOfferDefinition basePriceOffer = customPriceOffer with { CustomPriceCopper = null };
  if (basePriceOffer.EffectivePriceCopper != 100 || customPriceOffer.EffectivePriceCopper != 25)
  {
    throw new InvalidOperationException(
      "Shop offer price resolution did not preserve custom-or-base source semantics.");
  }

  ShopOfferDefinition invalidPriceOffer = customPriceOffer with { CustomPriceCopper = -1 };
  InventoryComponent invalidInventory = new();
  invalidInventory.SetSlot(0, new ItemStack(72, 1));
  long revisionBeforeInvalid = invalidInventory.Revision;
  ShopPurchaseResult invalid = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customPriceOffer.OfferId,
      "custom-price-session",
      2),
    interaction,
    invalidPriceOffer,
    invalidInventory,
    definitions,
    new ShopPurchaseState());
  if (invalid.IsAccepted || invalidInventory.Revision != revisionBeforeInvalid ||
      invalidInventory.GetSlot(0) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "An invalid custom shop price did not fail closed before inventory mutation.");
  }

  Console.WriteLine(
    "PASS: shop custom price overrides base price and rejects invalid values atomically");
}

static void VerifyShopSpecialCurrencyPayment()
{
  const int defenderMedalCurrencyId = 0;
  const ushort defenderMedalItemType = 3817;
  ItemDefinitionRegistry definitions = new([
    new ItemDefinition(defenderMedalItemType, 999),
    new ItemDefinition(200, 10)]);
  CustomCurrencyDefinitionRegistry currencies = new([
    new CustomCurrencyDefinition(defenderMedalCurrencyId, defenderMedalItemType, 999)]);
  ShopOfferDefinition specialCurrencyOffer = new(
    802,
    200,
    1,
    100,
    BuyOnce: true,
    CustomPriceCopper: 5,
    SpecialCurrencyId: defenderMedalCurrencyId);
  Dictionary<int, ShopOfferDefinition> offers = new()
  {
    [specialCurrencyOffer.OfferId] = specialCurrencyOffer
  };
  ShopPurchaseSystem purchaseSystem = new(offers, currencies);
  ShopPurchaseState purchaseState = new();
  NpcInteractionComponent interaction = new();
  interaction.Apply(
    new NpcInteractionCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      NpcInteractionKind.Shop,
      "special-currency-session",
      new SimulationVector(0.0f, 0.0f)),
    1);
  InventoryComponent inventory = new();
  inventory.SetSlot(0, new ItemStack(defenderMedalItemType, 8));
  ShopPurchaseResult result = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialCurrencyOffer.OfferId,
      "special-currency-session",
      1),
    interaction,
    specialCurrencyOffer,
    inventory,
    definitions,
    purchaseState);
  if (!result.IsAccepted || inventory.GetSlot(0) != new ItemStack(defenderMedalItemType, 3) ||
      inventory.GetSlot(1) != new ItemStack(200, 1))
  {
    throw new InvalidOperationException(
      "A special-currency shop offer did not atomically consume its accepted currency item.");
  }

  InventoryComponent fullInventory = new();
  fullInventory.SetSlot(0, new ItemStack(defenderMedalItemType, 5));
  for (int slot = 1; slot < InventoryComponent.SlotCount; slot++)
  {
    fullInventory.SetSlot(slot, new ItemStack(defenderMedalItemType, 1));
  }

  ShopPurchaseResult freedCurrencySlot = new ShopPurchaseSystem(offers, currencies).TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialCurrencyOffer.OfferId,
      "special-currency-session",
      2),
    interaction,
    specialCurrencyOffer,
    fullInventory,
    definitions,
    new ShopPurchaseState());
  if (!freedCurrencySlot.IsAccepted ||
      fullInventory.GetSlot(0) != new ItemStack(200, 1) ||
      fullInventory.GetSlot(1) != new ItemStack(defenderMedalItemType, 1))
  {
    throw new InvalidOperationException(
      "A fully consumed special-currency stack did not provide an atomic product slot.");
  }

  InventoryComponent repeatedInventory = new();
  repeatedInventory.SetSlot(0, new ItemStack(defenderMedalItemType, 8));
  long repeatedRevision = repeatedInventory.Revision;
  ShopPurchaseResult repeated = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialCurrencyOffer.OfferId,
      "special-currency-session",
      2),
    interaction,
    specialCurrencyOffer,
    repeatedInventory,
    definitions,
    purchaseState);
  if (repeated.IsAccepted || repeatedInventory.Revision != repeatedRevision ||
      repeatedInventory.GetSlot(0) != new ItemStack(defenderMedalItemType, 8))
  {
    throw new InvalidOperationException(
      "BuyOnce did not remain enforced after a special-currency purchase.");
  }

  InventoryComponent ordinaryCurrencyInventory = new();
  ordinaryCurrencyInventory.SetSlot(0, new ItemStack(71, 999));
  long ordinaryRevision = ordinaryCurrencyInventory.Revision;
  ShopPurchaseResult missingSpecialCurrency = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialCurrencyOffer.OfferId,
      "special-currency-session",
      3),
    interaction,
    specialCurrencyOffer,
    ordinaryCurrencyInventory,
    definitions,
    new ShopPurchaseState());
  if (missingSpecialCurrency.IsAccepted ||
      ordinaryCurrencyInventory.Revision != ordinaryRevision ||
      ordinaryCurrencyInventory.GetSlot(0) != new ItemStack(71, 999))
  {
    throw new InvalidOperationException(
      "A special-currency offer incorrectly charged ordinary coins or mutated on rejection.");
  }

  ShopOfferDefinition unknownCurrencyOffer = specialCurrencyOffer with
  {
    OfferId = 803,
    SpecialCurrencyId = 99
  };
  Dictionary<int, ShopOfferDefinition> unknownOffers = new()
  {
    [unknownCurrencyOffer.OfferId] = unknownCurrencyOffer
  };
  ShopPurchaseSystem unknownPurchaseSystem = new(unknownOffers, currencies);
  InventoryComponent unknownCurrencyInventory = new();
  unknownCurrencyInventory.SetSlot(0, new ItemStack(defenderMedalItemType, 8));
  long unknownRevision = unknownCurrencyInventory.Revision;
  ShopPurchaseResult unknownCurrency = unknownPurchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      unknownCurrencyOffer.OfferId,
      "special-currency-session",
      4),
    interaction,
    unknownCurrencyOffer,
    unknownCurrencyInventory,
    definitions,
    new ShopPurchaseState());
  if (unknownCurrency.IsAccepted || unknownCurrencyInventory.Revision != unknownRevision ||
      unknownCurrencyInventory.GetSlot(0) != new ItemStack(defenderMedalItemType, 8))
  {
    throw new InvalidOperationException(
      "An unregistered special currency did not fail closed before inventory mutation.");
  }

  Console.WriteLine(
    "PASS: special-currency offers consume only registered currency items atomically");
}

static void VerifyShopPurchaseOnPurchaseCleanup()
{
  const int defenderMedalCurrencyId = 0;
  const ushort defenderMedalItemType = 3817;
  ItemDefinitionRegistry definitions = new([
    new ItemDefinition(71, 9999),
    new ItemDefinition(72, 9999),
    new ItemDefinition(defenderMedalItemType, 999),
    new ItemDefinition(200, 10)]);
  CustomCurrencyDefinitionRegistry currencies = new([
    new CustomCurrencyDefinition(defenderMedalCurrencyId, defenderMedalItemType, 999)]);
  ShopOfferDefinition customOrdinaryOffer = new(
    804,
    200,
    1,
    100,
    CustomPriceCopper: 25);
  ShopOfferDefinition customSpecialOffer = new(
    805,
    200,
    1,
    100,
    CustomPriceCopper: 5,
    SpecialCurrencyId: defenderMedalCurrencyId);
  ShopOfferDefinition specialWithoutCustomPrice = new(
    806,
    200,
    1,
    100,
    SpecialCurrencyId: defenderMedalCurrencyId);
  Dictionary<int, ShopOfferDefinition> offers = new()
  {
    [customOrdinaryOffer.OfferId] = customOrdinaryOffer,
    [customSpecialOffer.OfferId] = customSpecialOffer,
    [specialWithoutCustomPrice.OfferId] = specialWithoutCustomPrice
  };
  ShopPurchaseSystem purchaseSystem = new(offers, currencies);
  NpcInteractionComponent interaction = new();
  interaction.Apply(
    new NpcInteractionCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      NpcInteractionKind.Shop,
      "on-purchase-session",
      new SimulationVector(0.0f, 0.0f)),
    1);

  InventoryComponent ordinaryInventory = new();
  ordinaryInventory.SetSlot(0, new ItemStack(72, 1));
  ShopPurchaseResult ordinaryResult = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customOrdinaryOffer.OfferId,
      "on-purchase-session",
      1),
    interaction,
    customOrdinaryOffer,
    ordinaryInventory,
    definitions,
    new ShopPurchaseState());
  if (!ordinaryResult.IsAccepted ||
      ordinaryResult.Receipt is not ShopPurchaseReceipt ordinaryReceipt ||
      !ordinaryReceipt.ResetCustomPrice || !ordinaryReceipt.ResetSpecialCurrency ||
      ordinaryReceipt.OfferAfterCleanup.CustomPriceCopper is not null ||
      ordinaryReceipt.OfferAfterCleanup.SpecialCurrencyId != -1)
  {
    throw new InvalidOperationException(
      "A custom ordinary-currency purchase did not produce the source OnPurchase cleanup receipt.");
  }

  InventoryComponent specialInventory = new();
  specialInventory.SetSlot(0, new ItemStack(defenderMedalItemType, 8));
  ShopPurchaseResult specialResult = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customSpecialOffer.OfferId,
      "on-purchase-session",
      2),
    interaction,
    customSpecialOffer,
    specialInventory,
    definitions,
    new ShopPurchaseState());
  if (!specialResult.IsAccepted ||
      specialResult.Receipt is not ShopPurchaseReceipt specialReceipt ||
      !specialReceipt.ResetCustomPrice || !specialReceipt.ResetSpecialCurrency ||
      specialReceipt.OfferAfterCleanup.CustomPriceCopper is not null ||
      specialReceipt.OfferAfterCleanup.SpecialCurrencyId != -1)
  {
    throw new InvalidOperationException(
      "A custom special-currency purchase did not clear both source transient price fields.");
  }

  InventoryComponent specialWithoutCustomInventory = new();
  specialWithoutCustomInventory.SetSlot(0, new ItemStack(defenderMedalItemType, 100));
  ShopPurchaseResult specialWithoutCustomResult = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialWithoutCustomPrice.OfferId,
      "on-purchase-session",
      3),
    interaction,
    specialWithoutCustomPrice,
    specialWithoutCustomInventory,
    definitions,
    new ShopPurchaseState());
  if (!specialWithoutCustomResult.IsAccepted ||
      specialWithoutCustomResult.Receipt is not ShopPurchaseReceipt noResetReceipt ||
      noResetReceipt.ResetCustomPrice || noResetReceipt.ResetSpecialCurrency ||
      noResetReceipt.OfferAfterCleanup.SpecialCurrencyId != defenderMedalCurrencyId)
  {
    throw new InvalidOperationException(
      "OnPurchase cleanup changed a special-currency offer without a custom price.");
  }

  InventoryComponent rejectedInventory = new();
  rejectedInventory.SetSlot(0, new ItemStack(72, 1));
  long rejectedRevision = rejectedInventory.Revision;
  ShopPurchaseResult rejected = purchaseSystem.TryPurchase(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customOrdinaryOffer.OfferId,
      "forged-session",
      4),
    interaction,
    customOrdinaryOffer,
    rejectedInventory,
    definitions,
    new ShopPurchaseState());
  if (rejected.IsAccepted || rejected.Receipt is not null ||
      rejectedInventory.Revision != rejectedRevision ||
      rejectedInventory.GetSlot(0) != new ItemStack(72, 1))
  {
    throw new InvalidOperationException(
      "A rejected purchase produced a cleanup receipt or mutated inventory.");
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  NpcHandle npc = simulation.CreateNpc(
    new SimulationVector(50.0f, 0.0f),
    DomeSimulation.FixtureNpcType);
  simulation.RegisterShopOffer(customOrdinaryOffer with { OfferId = 807, ItemType = 1 });
  InventoryComponent simulationInventory = simulation.GetInventory(player);
  simulationInventory.SetSlot(0, new ItemStack(72, 1));
  simulation.QueueNpcInteraction(new NpcInteractionCommand(
    player,
    npc,
    NpcInteractionKind.Shop,
    "tick-on-purchase-session",
    new SimulationVector(50.0f, 0.0f)));
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    807,
    "tick-on-purchase-session",
    0));
  if (simulation.CreateShopPurchaseReceipts().Count != 0)
  {
    throw new InvalidOperationException(
      "A queued shop purchase published an OnPurchase receipt before the simulation tick.");
  }

  simulation.Tick(new SimulationInputBatch());
  IReadOnlyList<ShopPurchaseReceipt> receipts = simulation.CreateShopPurchaseReceipts();
  if (receipts.Count != 1 || receipts[0].Player != player || receipts[0].Npc != npc ||
      receipts[0].OfferId != 807 || receipts[0].Sequence != 0 ||
      !receipts[0].ResetCustomPrice || !receipts[0].ResetSpecialCurrency ||
      receipts[0].OfferAfterCleanup.CustomPriceCopper is not null)
  {
    throw new InvalidOperationException(
      "A successful shop purchase did not publish its cleanup receipt at tick commit.");
  }

  Console.WriteLine(
    "PASS: OnPurchase cleanup is source-guarded, atomic, and published only after tick commit");
}

static void VerifyShopCatalogCleanupConsumer()
{
  ShopOfferDefinition customOffer = new(
    808,
    200,
    1,
    100,
    CustomPriceCopper: 25);
  ShopOfferDefinition specialWithoutCustomPrice = new(
    809,
    200,
    1,
    100,
    SpecialCurrencyId: 0);
  Dictionary<int, ShopOfferDefinition> offers = new()
  {
    [customOffer.OfferId] = customOffer,
    [specialWithoutCustomPrice.OfferId] = specialWithoutCustomPrice
  };
  ShopOfferCatalogSystem catalog = new(offers);
  ShopPurchaseReceipt customReceipt = ShopPurchaseReceipt.Create(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      customOffer.OfferId,
      "catalog-session",
      1),
    customOffer);
  ShopPurchaseReceipt invalidReceipt = customReceipt with
  {
    OfferAfterCleanup = customReceipt.OfferAfterCleanup with
    {
      OfferId = customOffer.OfferId + 1000
    }
  };
  if (catalog.ApplyPurchaseReceipt(invalidReceipt) ||
      offers[customOffer.OfferId] != customOffer)
  {
    throw new InvalidOperationException(
      "The shop catalog accepted a cleanup receipt with mismatched offer identity.");
  }

  if (!catalog.ApplyPurchaseReceipt(customReceipt) ||
      offers[customOffer.OfferId].CustomPriceCopper is not null ||
      offers[customOffer.OfferId].SpecialCurrencyId != -1)
  {
    throw new InvalidOperationException(
      "The shop catalog did not consume a custom-price cleanup receipt.");
  }

  ShopPurchaseReceipt noResetReceipt = ShopPurchaseReceipt.Create(
    new ShopPurchaseCommand(
      new PlayerHandle(1),
      new NpcHandle(2),
      specialWithoutCustomPrice.OfferId,
      "catalog-session",
      2),
    specialWithoutCustomPrice);
  if (!catalog.ApplyPurchaseReceipt(noResetReceipt) ||
      offers[specialWithoutCustomPrice.OfferId] != specialWithoutCustomPrice)
  {
    throw new InvalidOperationException(
      "The shop catalog changed a special-currency offer without a custom price.");
  }

  Console.WriteLine(
    "PASS: shop catalog consumes OnPurchase cleanup receipts without widening the reset guard");
}

static void VerifyShopCatalogTickConsumer()
{
  const int offerId = 810;
  ShopOfferDefinition customOffer = new(
    offerId,
    1,
    1,
    100,
    CustomPriceCopper: 25);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  NpcHandle npc = simulation.CreateNpc(
    new SimulationVector(50.0f, 0.0f),
    DomeSimulation.FixtureNpcType);
  simulation.RegisterShopOffer(customOffer);
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(72, 2));

  simulation.QueueNpcInteraction(new NpcInteractionCommand(
    player,
    npc,
    NpcInteractionKind.Shop,
    "catalog-tick-session",
    new SimulationVector(50.0f, 0.0f)));
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    offerId,
    "catalog-tick-session",
    0));
  simulation.QueueShopPurchase(new ShopPurchaseCommand(
    player,
    npc,
    offerId,
    "catalog-tick-session",
    1));
  if (!simulation.TryGetShopOffer(offerId, out ShopOfferDefinition queuedOffer) ||
      queuedOffer != customOffer)
  {
    throw new InvalidOperationException(
      "A queued purchase changed the shop catalog before tick commit.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryGetShopOffer(offerId, out ShopOfferDefinition committedOffer) ||
      committedOffer.CustomPriceCopper is not null || committedOffer.SpecialCurrencyId != -1)
  {
    throw new InvalidOperationException(
      "A committed cleanup receipt did not update the authoritative shop catalog.");
  }

  IReadOnlyList<ShopPurchaseReceipt> receipts = simulation.CreateShopPurchaseReceipts();
  if (receipts.Count != 2 || !receipts[0].ResetCustomPrice || receipts[1].ResetCustomPrice ||
      inventory.GetSlot(1) != new ItemStack(1, 1) ||
      inventory.GetSlot(3) != new ItemStack(1, 1))
  {
    throw new InvalidOperationException(
      "A same-tick follow-up purchase did not observe the cleaned shop offer.");
  }

  using DomeSimulation rejectedSimulation = new(new WorldGrid(400, 300));
  PlayerHandle rejectedPlayer = rejectedSimulation.CreatePlayer(
    new SimulationVector(50.0f, 0.0f));
  NpcHandle rejectedNpc = rejectedSimulation.CreateNpc(
    new SimulationVector(50.0f, 0.0f),
    DomeSimulation.FixtureNpcType);
  rejectedSimulation.RegisterShopOffer(customOffer with { OfferId = offerId + 1 });
  rejectedSimulation.GetInventory(rejectedPlayer).SetSlot(0, new ItemStack(72, 1));
  rejectedSimulation.QueueShopPurchase(new ShopPurchaseCommand(
    rejectedPlayer,
    rejectedNpc,
    offerId + 1,
    "forged-session",
    0));
  rejectedSimulation.Tick(new SimulationInputBatch());
  if (!rejectedSimulation.TryGetShopOffer(
        offerId + 1,
        out ShopOfferDefinition rejectedOffer) ||
      rejectedOffer.CustomPriceCopper is null ||
      rejectedSimulation.CreateShopPurchaseReceipts().Count != 0)
  {
    throw new InvalidOperationException(
      "A rejected purchase changed the shop catalog or published a receipt.");
  }

  Console.WriteLine(
    "PASS: shop catalog cleanup is tick-bound, authoritative, and visible to same-tick purchases");
}

static void VerifyShopCatalogGeneratedRefresh()
{
  const int staleOfferId = 811;
  const int generatedOfferId = 812;
  ShopOfferDefinition firstGeneratedOffer = new(generatedOfferId, 1, 1, 150);
  ShopOfferDefinition secondGeneratedOffer = new(generatedOfferId + 1, 1, 1, 175);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  simulation.RegisterShopOffer(new ShopOfferDefinition(staleOfferId, 1, 1, 100));

  if (!simulation.TryRefreshGeneratedShopCatalog([
        firstGeneratedOffer,
        secondGeneratedOffer
      ]))
  {
    throw new InvalidOperationException(
      "A valid generated shop catalog was not accepted atomically.");
  }

  if (simulation.TryGetShopOffer(staleOfferId, out _) ||
      !simulation.TryGetShopOffer(generatedOfferId, out ShopOfferDefinition generatedOffer) ||
      generatedOffer != firstGeneratedOffer ||
      !simulation.TryGetShopOffer(
        secondGeneratedOffer.OfferId,
        out ShopOfferDefinition secondOffer) ||
      secondOffer != secondGeneratedOffer)
  {
    throw new InvalidOperationException(
      "Generated shop catalog refresh did not replace stale offers deterministically.");
  }

  ShopOfferDefinition preservedOffer = default;
  ShopOfferDefinition preservedSecondOffer = default;
  if (simulation.TryRefreshGeneratedShopCatalog([
        firstGeneratedOffer with { OfferId = generatedOfferId + 2 },
        new ShopOfferDefinition(generatedOfferId + 3, 65535, 1, 150)
      ]) ||
      !simulation.TryGetShopOffer(generatedOfferId, out preservedOffer) ||
      preservedOffer != firstGeneratedOffer ||
      !simulation.TryGetShopOffer(secondGeneratedOffer.OfferId, out preservedSecondOffer) ||
      preservedSecondOffer != secondGeneratedOffer)
  {
    throw new InvalidOperationException(
      "A partially invalid generated catalog did not fail closed without partial writes.");
  }

  if (simulation.TryRefreshGeneratedShopCatalog([
        firstGeneratedOffer,
        firstGeneratedOffer
      ]) ||
      !simulation.TryGetShopOffer(generatedOfferId, out preservedOffer) ||
      preservedOffer != firstGeneratedOffer ||
      !simulation.TryGetShopOffer(secondGeneratedOffer.OfferId, out preservedSecondOffer) ||
      preservedSecondOffer != secondGeneratedOffer)
  {
    throw new InvalidOperationException(
      "Duplicate generated offer IDs did not fail closed without catalog writes.");
  }

  ShopOfferDefinition preservedAfterUnknownItem = default;
  if (simulation.TryRefreshGeneratedShopCatalog([
        new ShopOfferDefinition(generatedOfferId, 65535, 1, 150)
      ]) ||
      !simulation.TryGetShopOffer(generatedOfferId, out preservedAfterUnknownItem) ||
      preservedAfterUnknownItem != firstGeneratedOffer)
  {
    throw new InvalidOperationException(
      "A generated offer with an unknown ItemDefinition changed the shop catalog.");
  }

  if (!simulation.TryRefreshGeneratedShopCatalog(Array.Empty<ShopOfferDefinition>()) ||
      simulation.TryGetShopOffer(generatedOfferId, out _) ||
      simulation.TryGetShopOffer(secondGeneratedOffer.OfferId, out _))
  {
    throw new InvalidOperationException(
      "An empty generated shop catalog did not clear the previous cache atomically.");
  }

  Console.WriteLine(
    "PASS: generated shop catalog refresh validates candidates before atomic replacement");
}

static void VerifyCartTrackPlacementOwner()
{
  const short cartTrackTileType = ItemPlacementDefinition.CartTrackTileType;
  ItemDefinition cartTrackDefinition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(
      ItemType: 2340,
      MaxStack: 99,
      TileType: cartTrackTileType,
      TileBoost: 5,
      Consumable: true,
      CartTrack: true));
  if (cartTrackDefinition.Placement is not ItemPlacementDefinition placement ||
      !placement.CartTrack || placement.TileType != cartTrackTileType || placement.WallType >= 0)
  {
    throw new InvalidOperationException(
      "Legacy cartTrack metadata was not mapped to a tile-only placement owner.");
  }

  ItemPlacementSystem placementSystem = new();
  if (!placementSystem.TryCreateTileCommand(
        cartTrackDefinition,
        3,
        4,
        1,
        out TileChangeCommand cartTrackCommand,
        out _) ||
      cartTrackCommand.Kind != TileChangeKind.Place ||
      cartTrackCommand.TileType != cartTrackTileType ||
      !cartTrackCommand.IsCartTrack || cartTrackCommand.WallType != 0)
  {
    throw new InvalidOperationException(
      "CartTrack placement did not emit a track-tagged tile command.");
  }

  WorldGrid world = new(400, 300);
  TileChangeCommitSystem commitSystem = new();
  WorldTile beforeForgedCommand = world.GetTile(5, 6);
  long versionBeforeForgedCommand = world.GetSectionVersion(world.GetSectionCoordinates(5, 6));
  TileChangeCommand forgedWrongType = new(
    2,
    5,
    6,
    TileChangeKind.Place,
    TileType: 315,
    IsCartTrack: true);
  if (commitSystem.TryCommit(world, [forgedWrongType], out _) ||
      world.GetTile(5, 6) != beforeForgedCommand ||
      world.GetSectionVersion(world.GetSectionCoordinates(5, 6)) != versionBeforeForgedCommand)
  {
    throw new InvalidOperationException(
      "A CartTrack command with a non-track tile type mutated world state.");
  }

  TileChangeCommand forgedWall = new(
    3,
    5,
    6,
    TileChangeKind.SetWall,
    TileType: 0,
    WallType: 7,
    IsCartTrack: true);
  if (commitSystem.TryCommit(world, [forgedWall], out _))
  {
    throw new InvalidOperationException(
      "A CartTrack command was accepted through the wall placement path.");
  }

  if (!commitSystem.TryCommit(world, [cartTrackCommand], out TileChangeCommitResult result) ||
      !result.Succeeded || world.GetTile(3, 4).Type != cartTrackTileType ||
      !world.GetTile(3, 4).IsActive)
  {
    throw new InvalidOperationException(
      "A valid CartTrack tile command did not commit at the world boundary.");
  }

  try
  {
    ItemDefinitionCompiler.Validate(new ItemDefinition(
      2341,
      99,
      Placement: new ItemPlacementDefinition(TileType: 315, CartTrack: true)));
    throw new InvalidOperationException(
      "The item compiler accepted CartTrack metadata with the wrong tile type.");
  }
  catch (ArgumentException)
  {
  }

  try
  {
    ItemDefinitionCompiler.Validate(new ItemDefinition(
      2342,
      99,
      Placement: new ItemPlacementDefinition(WallType: 7, CartTrack: true)));
    throw new InvalidOperationException(
      "The item compiler accepted CartTrack metadata on a wall placement.");
  }
  catch (ArgumentException)
  {
  }

  Console.WriteLine(
    "PASS: cartTrack owns tile 314 placement and rejects forged or wall commands");
}

static void VerifyPotionDelayAuthority()
{
  if (ItemDefinition.PotionDelayTicks != 3600)
  {
    throw new InvalidOperationException(
      "The source-backed potion delay constant did not preserve 3600 ticks.");
  }

  ItemDefinition potionDefinition = new(
    DomeSimulation.FixtureItemPotionType,
    20,
    Use: new ItemUseDefinition(
      UseTime: 1,
      UseAnimation: 1,
      Potion: true,
      Consumable: true,
      HealthRestore: 25));
  Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent firstUseState = new();
  ItemUseResult firstUse = new ItemUseSystem().TryUse(
    new PlayerHandle(1),
    0,
    new ItemStack(DomeSimulation.FixtureItemPotionType, 2),
    potionDefinition,
    ref firstUseState,
    health: 50,
    maximumHealth: 100,
    mana: 20,
    maximumMana: 20,
    sequence: 1,
    potionDelayTicks: 0);
  if (!firstUse.IsAccepted || firstUse.PotionDelayTicks != ItemDefinition.PotionDelayTicks)
  {
    throw new InvalidOperationException(
      "The first Potion use did not request the source-backed potion delay.");
  }

  Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent blockedUseState = new();
  ItemUseResult blockedUse = new ItemUseSystem().TryUse(
    new PlayerHandle(1),
    0,
    new ItemStack(DomeSimulation.FixtureItemPotionType, 2),
    potionDefinition,
    ref blockedUseState,
    health: 50,
    maximumHealth: 100,
    mana: 20,
    maximumMana: 20,
    sequence: 2,
    potionDelayTicks: firstUse.PotionDelayTicks);
  if (blockedUse.IsAccepted || blockedUseState.UseRevision != 0 ||
      blockedUse.PotionDelayTicks != 0)
  {
    throw new InvalidOperationException(
      "Potion use was not rejected while the authoritative delay was active.");
  }

  Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent ordinaryUseState = new();
  ItemUseResult ordinaryUse = new ItemUseSystem().TryUse(
    new PlayerHandle(1),
    0,
    new ItemStack(21, 1),
    new ItemDefinition(
      21,
      20,
      Use: new ItemUseDefinition(UseTime: 1, HealthRestore: 5, Consumable: true)),
    ref ordinaryUseState,
    health: 50,
    maximumHealth: 100,
    mana: 20,
    maximumMana: 20,
    sequence: 4,
    potionDelayTicks: ItemDefinition.PotionDelayTicks);
  if (!ordinaryUse.IsAccepted || ordinaryUseState.UseRevision != 1)
  {
    throw new InvalidOperationException(
      "A non-Potion item was incorrectly blocked by potion sickness.");
  }

  Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent forgedUseState = new();
  ItemUseResult forgedUse = new ItemUseSystem().TryUse(
    new PlayerHandle(1),
    0,
    new ItemStack(DomeSimulation.FixtureItemPotionType, 2),
    potionDefinition,
    ref forgedUseState,
    health: 50,
    maximumHealth: 100,
    mana: 20,
    maximumMana: 20,
    sequence: 3,
    potionDelayTicks: -1);
  if (forgedUse.IsAccepted || forgedUseState.UseRevision != 0)
  {
    throw new InvalidOperationException(
      "A negative forged potion delay was not rejected without use-state mutation.");
  }

  PlayerPotionStateComponent directPotionState = new();
  directPotionState.Apply(ItemDefinition.PotionDelayTicks);
  directPotionState.Tick();
  if (directPotionState.PotionDelayTicks != ItemDefinition.PotionDelayTicks - 1)
  {
    throw new InvalidOperationException(
      "Potion delay did not decrement exactly once per direct tick.");
  }

  directPotionState.PotionDelayTicks = -1;
  directPotionState.Tick();
  if (directPotionState.PotionDelayTicks != 0)
  {
    throw new InvalidOperationException(
      "A forged negative potion delay did not fail closed to zero.");
  }

  PlayerPotionDelaySystem potionDelaySystem = new();
  BuffCollectionComponent duplicatePotionBuffs = new();
  PlayerHandle duplicatePotionPlayer = new(1);
  duplicatePotionBuffs.Add(
    PlayerPotionStateComponent.PotionSicknessBuffType,
    3,
    duplicatePotionPlayer);
  duplicatePotionBuffs.Add(22, 5, duplicatePotionPlayer);
  duplicatePotionBuffs.SetAt(
    1,
    new BuffEntry(PlayerPotionStateComponent.PotionSicknessBuffType, 9, duplicatePotionPlayer));
  PlayerPotionStateComponent duplicatePotionState = new();
  potionDelaySystem.SynchronizeFromBuffs(ref duplicatePotionState, duplicatePotionBuffs);
  if (duplicatePotionState.PotionDelayTicks != 9 ||
      !potionDelaySystem.IsBlocked(potionDefinition, duplicatePotionState.PotionDelayTicks))
  {
    throw new InvalidOperationException(
      "Forged duplicate Buff 21 entries did not fail closed to the longest active delay.");
  }

  BuffCollectionComponent reservedPotionBuffs = new();
  for (ushort type = 100; type < 143; type++)
  {
    reservedPotionBuffs.Add(type, 100, duplicatePotionPlayer);
  }

  PlayerPotionStateComponent reservedPotionState = new();
  if (potionDelaySystem.TryApplyPotionUse(
        ref reservedPotionState,
        reservedPotionBuffs,
        duplicatePotionPlayer,
        reservedBuffSlots: 1) ||
      reservedPotionState.PotionDelayTicks != 0 ||
      reservedPotionBuffs.Count != 43)
  {
    throw new InvalidOperationException(
      "Potion preflight did not preserve state when a reserved Buff slot was unavailable.");
  }

  BuffCollectionComponent nonThrowingBuffs = new(1);
  if (!nonThrowingBuffs.TryAdd(100, 30, duplicatePotionPlayer) ||
      nonThrowingBuffs.TryAdd(101, 30, duplicatePotionPlayer))
  {
    throw new InvalidOperationException(
      "Buff collection TryAdd did not provide a non-throwing capacity boundary.");
  }

  try
  {
    _ = new ItemUseSnapshot(
      new PlayerHandle(1),
      new Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent(),
      0,
      potionDelayTicks: -1);
    throw new InvalidOperationException(
      "The item-use snapshot accepted a negative potion delay.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(30.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(DomeSimulation.FixtureItemPotionType, 2));
  simulation.QueuePlayerDamage(player, 50);
  simulation.Tick(new SimulationInputBatch());
  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));

  PlayerStateSnapshot firstSnapshot = simulation.CreatePlayerStateSnapshot(player);
  BuffCollectionComponent buffs = GetPlayerBuffCollection(simulation, player);
  bool hasPotionSicknessBuff = buffs.Entries.Count(
    entry => entry.Type == PlayerPotionStateComponent.PotionSicknessBuffType) == 1;
  if (firstSnapshot.PotionDelayTicks != ItemDefinition.PotionDelayTicks ||
      simulation.CreateItemUseSnapshot(player).PotionDelayTicks !=
        ItemDefinition.PotionDelayTicks ||
      inventory.GetSlot(0) != new ItemStack(DomeSimulation.FixtureItemPotionType, 1) ||
      !hasPotionSicknessBuff)
  {
    throw new InvalidOperationException(
      "A committed Potion use did not publish player delay and Buff 21 atomically.");
  }

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: false)));
  int delayAfterOneTick = simulation.CreatePlayerStateSnapshot(player).PotionDelayTicks;
  if (delayAfterOneTick != ItemDefinition.PotionDelayTicks - 1 ||
      simulation.CreateItemUseSnapshot(player).PotionDelayTicks != delayAfterOneTick)
  {
    throw new InvalidOperationException(
      "The server-owned potion delay did not decrement once on the next tick.");
  }

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  if (simulation.CreateItemUsedEvents().Count != 0 ||
      inventory.GetSlot(0) != new ItemStack(DomeSimulation.FixtureItemPotionType, 1) ||
      simulation.CreatePlayerStateSnapshot(player).PotionDelayTicks != delayAfterOneTick - 1)
  {
    throw new InvalidOperationException(
      "Potion sickness did not block a second use without inventory mutation.");
  }

  Arch.Core.Entity playerEntity = FindPlayerEntity(simulation, player);
  ref PlayerPotionStateComponent runtimePotionState =
    ref simulation.World.Get<PlayerPotionStateComponent>(playerEntity);
  runtimePotionState.PotionDelayTicks = 1;
  buffs.SetAt(
    buffs.Entries.ToList().FindIndex(
      entry => entry.Type == PlayerPotionStateComponent.PotionSicknessBuffType),
    new BuffEntry(PlayerPotionStateComponent.PotionSicknessBuffType, 1, player));
  simulation.Tick(new SimulationInputBatch());
  if (simulation.CreatePlayerStateSnapshot(player).PotionDelayTicks != 0 ||
      buffs.Entries.Any(entry => entry.Type == PlayerPotionStateComponent.PotionSicknessBuffType))
  {
    throw new InvalidOperationException(
      "Potion delay did not clear when Buff 21 expired.");
  }

  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  if (simulation.CreateItemUsedEvents().Count != 1 || !inventory.GetSlot(0).IsEmpty ||
      simulation.CreatePlayerStateSnapshot(player).PotionDelayTicks !=
        ItemDefinition.PotionDelayTicks ||
      simulation.CreateItemUseSnapshot(player).PotionDelayTicks != ItemDefinition.PotionDelayTicks)
  {
    throw new InvalidOperationException(
      $"Potion use did not become available again after the delay reached zero. " +
      $"events={simulation.CreateItemUsedEvents().Count}, stack={inventory.GetSlot(0)}, " +
      $"delay={simulation.CreatePlayerStateSnapshot(player).PotionDelayTicks}, " +
      $"revision={simulation.CreateItemUseSnapshot(player).State.UseRevision}, " +
      $"cooldown={simulation.CreateItemUseSnapshot(player).State.CooldownTicks}");
  }

  using DomeSimulation buffProjectionSimulation = new(new WorldGrid(400, 300));
  PlayerHandle buffProjectionPlayer = buffProjectionSimulation.CreatePlayer(
    new SimulationVector(40.0f, 0.0f));
  BuffCollectionComponent buffProjection = GetPlayerBuffCollection(
    buffProjectionSimulation,
    buffProjectionPlayer);
  buffProjection.Add(
    PlayerPotionStateComponent.PotionSicknessBuffType,
    9,
    buffProjectionPlayer);
  buffProjectionSimulation.Tick(new SimulationInputBatch());
  if (buffProjectionSimulation.CreatePlayerStateSnapshot(buffProjectionPlayer)
      .PotionDelayTicks != 8)
  {
    throw new InvalidOperationException(
      "Buff 21 was not projected into the player potion delay owner.");
  }

  using DomeSimulation fullBuffSimulation = new(new WorldGrid(400, 300));
  PlayerHandle buffSource = fullBuffSimulation.CreatePlayer(new SimulationVector(60.0f, 0.0f));
  PlayerHandle fullBuffPlayer = fullBuffSimulation.CreatePlayer(
    new SimulationVector(61.0f, 0.0f));
  BuffCollectionComponent fullBuffCollection = GetPlayerBuffCollection(
    fullBuffSimulation,
    fullBuffPlayer);
  for (ushort type = 100; type < 144; type++)
  {
    fullBuffCollection.Add(type, 100, buffSource);
  }

  if (!fullBuffSimulation.TryQueuePlayerPvpBuff(buffSource, fullBuffPlayer, 20, 30))
  {
    throw new InvalidOperationException(
      "A valid target status command was not accepted before capacity commit.");
  }

  fullBuffSimulation.Tick(new SimulationInputBatch());
  if (fullBuffCollection.Count != fullBuffCollection.MaximumCount ||
      fullBuffCollection.Entries.Any(entry => entry.Type == 20))
  {
    throw new InvalidOperationException(
      "A target status effect exceeded the full Buff collection capacity.");
  }

  using DomeSimulation deathSimulation = new(new WorldGrid(400, 300));
  PlayerHandle deadPlayer = deathSimulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  Arch.Core.Entity deadPlayerEntity = FindPlayerEntity(deathSimulation, deadPlayer);
  ref PlayerPotionStateComponent deadPotionState =
    ref deathSimulation.World.Get<PlayerPotionStateComponent>(deadPlayerEntity);
  deadPotionState.Apply(ItemDefinition.PotionDelayTicks);
  BuffCollectionComponent deadPlayerBuffs = GetPlayerBuffCollection(deathSimulation, deadPlayer);
  deadPlayerBuffs.Add(
    PlayerPotionStateComponent.PotionSicknessBuffType,
    ItemDefinition.PotionDelayTicks,
    deadPlayer);
  deathSimulation.QueuePlayerDamage(deadPlayer, 1000);
  deathSimulation.Tick(new SimulationInputBatch());
  PlayerStateSnapshot deadSnapshot = deathSimulation.CreatePlayerStateSnapshot(deadPlayer);
  if (deadSnapshot.IsActive || deadSnapshot.PotionDelayTicks != 0 ||
      deadPlayerBuffs.Entries.Any(
        entry => entry.Type == PlayerPotionStateComponent.PotionSicknessBuffType))
  {
    throw new InvalidOperationException(
      "Player death did not clear the authoritative Potion delay and Buff 21.");
  }

  deadPotionState.PotionDelayTicks = ItemDefinition.PotionDelayTicks;
  deadPlayerBuffs.Add(
    PlayerPotionStateComponent.PotionSicknessBuffType,
    ItemDefinition.PotionDelayTicks,
    deadPlayer);
  deathSimulation.Tick(new SimulationInputBatch());
  if (deathSimulation.CreatePlayerStateSnapshot(deadPlayer).PotionDelayTicks != 0 ||
      deadPlayerBuffs.Entries.Any(
        entry => entry.Type == PlayerPotionStateComponent.PotionSicknessBuffType))
  {
    throw new InvalidOperationException(
      "A dead player retained a forged Potion delay or Buff 21 across a death tick.");
  }

  deathSimulation.Tick(new SimulationInputBatch());
  deathSimulation.Tick(new SimulationInputBatch());
  deathSimulation.Tick(new SimulationInputBatch());
  if (!deathSimulation.CreatePlayerStateSnapshot(deadPlayer).IsActive ||
      deathSimulation.CreatePlayerStateSnapshot(deadPlayer).PotionDelayTicks != 0)
  {
    throw new InvalidOperationException(
      "Player respawn did not restore an active player with no Potion delay.");
  }

  Console.WriteLine(
    "PASS: potionDelay is player-owned, Buff 21-backed, ticked once and fail-closed");
}

static void VerifyLegacyFlaskDurationOwner()
{
  (ushort itemType, ushort buffType)[] expectedMappings =
  [
    (1340, 71),
    (1353, 73),
    (1354, 74),
    (1355, 75),
    (1356, 76),
    (1357, 77),
    (1358, 78),
    (1359, 79)
  ];
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  for (int index = 0; index < expectedMappings.Length; index++)
  {
    (ushort itemType, ushort expectedBuffType) = expectedMappings[index];
    if (!simulation.ItemDefinitions.TryGet(itemType, out ItemDefinition definition) ||
        definition.Use is not ItemUseDefinition use ||
        definition.Recovery is not ItemRecoveryDefinition recovery ||
        !use.Consumable || use.UseStyle != 9 || use.UseTime != 17 ||
        use.UseAnimation != 17 || !use.UseTurn ||
        recovery.BuffType != expectedBuffType ||
        recovery.BuffDurationTicks != ItemDefinition.FlaskDurationTicks)
    {
      throw new InvalidOperationException(
        $"DomeSimulation did not register source-backed Flask item {itemType}.");
    }
  }

  ItemDefinition adaptedDefinition = LegacyItemDefinitionAdapter.ToDefinition(
    new LegacyItemDefinitionRecord(ItemType: 1340, MaxStack: ItemDefinition.CommonMaxStack));
  if (!adaptedDefinition.IsConsumable ||
      adaptedDefinition.Recovery is not ItemRecoveryDefinition adaptedRecovery ||
      adaptedRecovery.BuffType != 71 ||
      adaptedRecovery.BuffDurationTicks != ItemDefinition.FlaskDurationTicks)
  {
    throw new InvalidOperationException(
      "The legacy Flask adapter did not apply the source duration owner.");
  }

  try
  {
    _ = LegacyItemDefinitionAdapter.ToDefinition(
      new LegacyItemDefinitionRecord(ItemType: 1340, MaxStack: 1, BuffType: 72));
    throw new InvalidOperationException(
      "The legacy Flask adapter accepted a conflicting source buff type.");
  }
  catch (ArgumentException)
  {
  }

  PlayerHandle player = simulation.CreatePlayer(new SimulationVector(50.0f, 0.0f));
  InventoryComponent inventory = simulation.GetInventory(player);
  inventory.SetSlot(0, new ItemStack(1340, 2));
  simulation.Tick(new SimulationInputBatch(new PlayerInput(
    player,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    Fire: false,
    SelectedSlot: 0,
    UseItem: true)));
  ItemUsedEvent[] events = simulation.CreateItemUsedEvents().ToArray();
  BuffCollectionComponent buffs = GetPlayerBuffCollection(simulation, player);
  if (events.Length != 1 || events[0].BuffType != 71 ||
      events[0].BuffDurationTicks != ItemDefinition.FlaskDurationTicks ||
      !events[0].ConsumedMainItem || inventory.GetSlot(0) != new ItemStack(1340, 1) ||
      buffs.Entries.Count(entry => entry.Type == 71) != 1 ||
      buffs.Entries.Single(entry => entry.Type == 71).RemainingTicks !=
        ItemDefinition.FlaskDurationTicks)
  {
    throw new InvalidOperationException(
      "A source-backed Flask use did not publish or apply the canonical duration.");
  }

  Console.WriteLine(
    "PASS: flaskTime is a source-backed Definition duration consumed by the server buff path");
}

static BuffCollectionComponent GetPlayerBuffCollection(
  DomeSimulation simulation,
  PlayerHandle player)
{
  Arch.Core.Entity playerEntity = FindPlayerEntity(simulation, player);
  return simulation.World.Get<BuffCollectionComponent>(playerEntity);
}

static Arch.Core.Entity FindPlayerEntity(DomeSimulation simulation, PlayerHandle player)
{
  Arch.Core.Entity playerEntity = default;
  bool found = false;
  Arch.Core.QueryDescription playerQuery = new Arch.Core.QueryDescription()
    .WithAll<PlayerIdentityComponent>();
  simulation.World.Query(
    in playerQuery,
    (Arch.Core.Entity entity, ref PlayerIdentityComponent identity) =>
    {
      if (identity.Player == player)
      {
        playerEntity = entity;
        found = true;
      }
    });
  if (!found)
  {
    throw new InvalidOperationException(
      "The player entity was not available for buff verification.");
  }

  return playerEntity;
}

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
