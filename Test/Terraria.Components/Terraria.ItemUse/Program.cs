using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Item;
using Terraria.Player;
using Terraria.Relationships;

// 物品使用 / 弹药 / 通道验证，对齐 Version4 Player.ItemCheck 的使用门闸与弹药消耗：
//   1. useTime 决定下一次可用 tick；未到点一律拒绝。
//   2. 远程武器每次射击消耗 1 发弹药，无弹药则拒绝且不改变状态。
//   3. channel 物品在使用期间保持通道标记，ReleaseUse 清除。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

using ArchWorld world = ArchWorld.Create();
_ = world.Create(new PlayerItemUseRequestQueueComponent());
_ = world.Create(new PlayerItemUseFactQueueComponent());

ItemDefinitionRegistry itemDefinitions = new();
foreach (ItemId itemId in new[] { ItemId.Torch, ItemId.CopperBow, ItemId.WoodenArrow, ItemId.MagicMissile, ItemId.Gel })
{
  itemDefinitions.Register(new ItemDefinition(itemId, 999, ItemCategoryMask.Any));
}

ItemUseDefinitionCatalog useDefinitions = new();
useDefinitions.Register(new ItemUseDefinition(
    ItemId.Torch, UseTime: 15, UseAnimation: 15, AutoReuse: true, Channel: false,
    UseAmmo: ItemId.None, ConsumeAmmo: false, ManaCost: 0));
useDefinitions.Register(new ItemUseDefinition(
    ItemId.CopperBow, UseTime: 25, UseAnimation: 25, AutoReuse: false, Channel: false,
    UseAmmo: ItemId.WoodenArrow, ConsumeAmmo: true, ManaCost: 0));
useDefinitions.Register(new ItemUseDefinition(
    ItemId.MagicMissile, UseTime: 12, UseAnimation: 12, AutoReuse: true, Channel: true,
    UseAmmo: ItemId.None, ConsumeAmmo: false, ManaCost: 10));
useDefinitions.Seal();

EntityRuntimeRegistry registry = new(worldScope: 1);
EntityUuid playerId = EntityUuid.Create();
EntityRuntimeHandle playerHandle = registry.Allocate(playerId);
ArchEntity player = world.Create();
world.Add(player, new EntityIdentityComponent(playerId));
world.Add(player, new InventoryComponent(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 999),
  new InventorySlotDefinition(1, InventorySlotKind.General, ItemCategoryMask.Any, 999),
  new InventorySlotDefinition(2, InventorySlotKind.General, ItemCategoryMask.Any, 999),
]));
registry.Register(playerHandle, player);

InventorySystem inventories = new(world, itemDefinitions);
_ = inventories.TrySetSlotValue(player, 0, new ItemStack(ItemId.CopperBow, 1));
_ = inventories.TrySetSlotValue(player, 1, new ItemStack(ItemId.WoodenArrow, 3));
_ = inventories.TrySetSlotValue(player, 2, new ItemStack(ItemId.Torch, 50));

PlayerItemUseSystem system = new(world, registry, useDefinitions, inventories);
PlayerItemUseRequestQueueComponent requests = PlayerItemUseRequestQueue.Require(world);
PlayerItemUseFactQueueComponent factQueue = PlayerItemUseFactQueue.Require(world);

(PlayerItemUseFact Fact, PlayerItemUseStateComponent State) Run(long tick, PlayerItemUseRequest request)
{
  requests.Enqueue(request);
  using EcsServerCommandBuffer commands = new();
  system.Update(new EcsServerTickContext(world, tick, 1f / 60f, commands));
  commands.Playback(world);
  IReadOnlyList<PlayerItemUseFact> facts = factQueue.Drain();
  PlayerItemUseStateComponent state = world.Has<PlayerItemUseStateComponent>(player)
      ? world.Get<PlayerItemUseStateComponent>(player)
      : PlayerItemUseStateComponent.Idle;
  return (facts.Count == 1 ? facts[0] : default, state);
}

PlayerItemUseRequest Begin(short slot) => new(PlayerItemUseRequestKind.BeginUse, playerHandle, slot);
PlayerItemUseRequest Release() => new(PlayerItemUseRequestKind.ReleaseUse, playerHandle, (short)-1);

// --- 1. 火把：使用门闸 ---
(PlayerItemUseFact torchFact, PlayerItemUseStateComponent torchState) = Run(0, Begin(2));
Check(
    "torch-use-applied",
    torchFact.Outcome == PlayerItemUseOutcomeKind.Applied && torchFact.ItemId == ItemId.Torch,
    $"outcome={torchFact.Outcome} item={torchFact.ItemId}");
Check(
    "torch-sets-ready-tick",
    torchState.ReadyTick == 15 && torchState.UseStartTick == 0 && !torchState.Channeling,
    $"readyTick={torchState.ReadyTick} channel={torchState.Channeling}");

(PlayerItemUseFact earlyFact, _) = Run(5, Begin(2));
Check(
    "use-gate-blocks-early",
    earlyFact.Outcome == PlayerItemUseOutcomeKind.NotReady,
    $"outcome={earlyFact.Outcome}");

(PlayerItemUseFact reopened, PlayerItemUseStateComponent reopenedState) = Run(15, Begin(2));
Check(
    "use-gate-opens-at-ready-tick",
    reopened.Outcome == PlayerItemUseOutcomeKind.Applied && reopenedState.ReadyTick == 30,
    $"outcome={reopened.Outcome} readyTick={reopenedState.ReadyTick}");

// --- 2. 弓：弹药消耗 ---
(PlayerItemUseFact bowFact, PlayerItemUseStateComponent bowState) = Run(100, Begin(0));
Check(
    "bow-use-consumes-ammo",
    bowFact.Outcome == PlayerItemUseOutcomeKind.Applied &&
        bowFact.ItemId == ItemId.CopperBow &&
        bowFact.AmmoItemId == ItemId.WoodenArrow,
    $"outcome={bowFact.Outcome} ammo={bowFact.AmmoItemId}");
Check(
    "bow-ammo-stack-decremented",
    GetStack(world, player, 1) is ItemStack arrows && arrows.Quantity == 2,
    $"arrows={(GetStack(world, player, 1) is ItemStack s ? s.Quantity : -1)}");
Check(
    "bow-ready-tick-from-use-time",
    bowState.ReadyTick == 125,
    $"readyTick={bowState.ReadyTick}");

_ = inventories.TrySetSlotValue(player, 1, default);
(PlayerItemUseFact noAmmoFact, PlayerItemUseStateComponent noAmmoState) = Run(200, Begin(0));
Check(
    "bow-without-ammo-rejected",
    noAmmoFact.Outcome == PlayerItemUseOutcomeKind.MissingAmmo && noAmmoState.ReadyTick == 125,
    $"outcome={noAmmoFact.Outcome} readyTick={noAmmoState.ReadyTick}");

// --- 3. 通道物品 ---
_ = inventories.TrySetSlotValue(player, 0, new ItemStack(ItemId.MagicMissile, 1));
(PlayerItemUseFact channelFact, PlayerItemUseStateComponent channelState) = Run(300, Begin(0));
Check(
    "channel-item-marks-channeling",
    channelFact.Outcome == PlayerItemUseOutcomeKind.Applied && channelState.Channeling,
    $"outcome={channelFact.Outcome} channel={channelState.Channeling}");

(PlayerItemUseFact continuedFact, _) = Run(400, Begin(0));
Check(
    "channel-reuse-is-continuous",
    continuedFact.Outcome == PlayerItemUseOutcomeKind.Applied && continuedFact.Continuous,
    $"outcome={continuedFact.Outcome} continuous={continuedFact.Continuous}");

(PlayerItemUseFact releaseFact, PlayerItemUseStateComponent releasedState) = Run(401, Release());
Check(
    "release-clears-channel",
    releaseFact.Outcome == PlayerItemUseOutcomeKind.Applied && !releasedState.Channeling,
    $"outcome={releaseFact.Outcome} channel={releasedState.Channeling}");

(PlayerItemUseFact releaseAgain, _) = Run(402, Release());
Check(
    "release-without-channel-rejected",
    releaseAgain.Outcome == PlayerItemUseOutcomeKind.NotChanneling,
    $"outcome={releaseAgain.Outcome}");

// --- 4. 拒绝路径 ---
_ = inventories.TrySetSlotValue(player, 2, new ItemStack(ItemId.Gel, 5));
(PlayerItemUseFact unknownFact, _) = Run(500, Begin(2));
Check(
    "unknown-item-rejected",
    unknownFact.Outcome == PlayerItemUseOutcomeKind.UnknownItem,
    $"outcome={unknownFact.Outcome}");

_ = inventories.TrySetSlotValue(player, 2, default);
(PlayerItemUseFact emptyFact, _) = Run(501, Begin(2));
Check(
    "empty-slot-rejected",
    emptyFact.Outcome == PlayerItemUseOutcomeKind.EmptySlot,
    $"outcome={emptyFact.Outcome}");

(PlayerItemUseFact outOfRangeFact, _) = Run(502, Begin(9));
Check(
    "unknown-slot-rejected",
    outOfRangeFact.Outcome == PlayerItemUseOutcomeKind.EmptySlot,
    $"outcome={outOfRangeFact.Outcome}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: item use timing, ammo consumption and channel state follow the reference gates.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;

static ItemStack? GetStack(ArchWorld world, ArchEntity player, int slotIndex)
{
  if (!world.Has<InventoryComponent>(player))
  {
    return null;
  }

  foreach (InventorySlot slot in world.Get<InventoryComponent>(player).Slots)
  {
    if (slot.Definition.Index == slotIndex && slot.Content is ItemStack stack)
    {
      return stack;
    }
  }

  return null;
}
