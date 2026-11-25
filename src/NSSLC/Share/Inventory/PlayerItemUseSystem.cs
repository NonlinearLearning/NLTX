using System;
using System.Collections.Generic;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Share.Relationships;

namespace Terraria.Share.Player;

/// <summary>
/// Applies item use requests, ported from the gate and ammo part of Version4 <c>Player.ItemCheck</c>.
/// </summary>
/// <remarks>
/// Per request the system replays the reference's order: does the slot hold a usable stack, is the
/// use gate open, is the required ammunition present, then commit the gate, the consumed ammo and
/// the channel flag and publish one fact. Mana spend is not applied: the ECS has no mana resource
/// yet, so <see cref="ItemUse.ManaCost"/> travels as data only and the gap is recorded
/// rather than guessed at.
/// </remarks>
public sealed class PlayerItemUseSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly ItemUseCatalog _definitions;
  private readonly InventorySystem _inventories;
  private PlayerItemUseFactQueueComponent? _facts;

  public PlayerItemUseSystem(
      ArchWorld ecsWorld,
      IEntityRuntimeResolver resolver,
      ItemUseCatalog definitions,
      InventorySystem inventories)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _inventories = inventories ?? throw new ArgumentNullException(nameof(inventories));
  }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private PlayerItemUseFactQueueComponent Facts
    => _facts ??= PlayerItemUseFactQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    PlayerItemUseRequestQueueComponent queue = PlayerItemUseRequestQueue.Require(World);
    IReadOnlyList<PlayerItemUseRequest> requests = queue.Drain();
    if (requests.Count == 0)
    {
      return;
    }

    PlayerItemUseFactQueueComponent facts = Facts;
    Dictionary<ArchEntity, PlayerItemUseStateComponent> pendingStates = [];
    for (int index = 0; index < requests.Count; index++)
    {
      Apply(requests[index], context.Tick, facts, pendingStates);
    }

    foreach (KeyValuePair<ArchEntity, PlayerItemUseStateComponent> pending in pendingStates)
    {
      if (World.Has<PlayerItemUseStateComponent>(pending.Key))
      {
        World.Get<PlayerItemUseStateComponent>(pending.Key) = pending.Value;
      }
      else if (context.Commands is not null)
      {
        context.Commands.Add(pending.Key, pending.Value);
      }
      else
      {
        throw new InvalidOperationException(
            "Player item-use state creation requires the ECS command buffer.");
      }
    }
  }

  private void Apply(
      in PlayerItemUseRequest request,
      long tick,
      PlayerItemUseFactQueueComponent facts,
      Dictionary<ArchEntity, PlayerItemUseStateComponent> pendingStates)
  {
    if (!_resolver.TryResolve(request.Player, out ArchEntity player) ||
        player == ArchEntity.Null || !World.IsAlive(player))
    {
      return;
    }

    // 状态组件按需挂载：同一 tick 内重复请求先在本地合并，最后统一 Add。
    PlayerItemUseStateComponent state = World.Has<PlayerItemUseStateComponent>(player)
        ? World.Get<PlayerItemUseStateComponent>(player)
        : pendingStates.GetValueOrDefault(player, PlayerItemUseStateComponent.Idle);
    if (request.Kind == PlayerItemUseRequestKind.ReleaseUse)
    {
      Release(request, state, player, tick, facts, pendingStates);
      return;
    }

    Begin(request, state, player, tick, facts, pendingStates);
  }

  private void Release(
      in PlayerItemUseRequest request,
      in PlayerItemUseStateComponent state,
      ArchEntity player,
      long tick,
      PlayerItemUseFactQueueComponent facts,
      Dictionary<ArchEntity, PlayerItemUseStateComponent> pendingStates)
  {
    bool wasChanneling = state.Channeling;
    SetState(player, state with { Channeling = false }, pendingStates);
    facts.Enqueue(new PlayerItemUseFact(
        request.Player,
        state.Slot,
        state.ActiveItem,
        state.ActiveAmmo,
        tick,
        Continuous: false,
        wasChanneling ? PlayerItemUseOutcomeKind.Applied : PlayerItemUseOutcomeKind.NotChanneling));
  }

  private void Begin(
      in PlayerItemUseRequest request,
      in PlayerItemUseStateComponent state,
      ArchEntity player,
      long tick,
      PlayerItemUseFactQueueComponent facts,
      Dictionary<ArchEntity, PlayerItemUseStateComponent> pendingStates)
  {
    if (!World.Has<InventoryComponent>(player))
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.NoInventory, facts);
      return;
    }

    // Version4 ItemCheck 第一行就是 Item item = inventory[selectedItem]（Player.cs:42925），
    // 用的始终是手持槽。服务端把这个关系表达在库存上，因此这里先确认请求指的正是手持槽，
    // 再从库存读出那一格的值——不在系统里另存一份"手上物品"。
    InventoryComponent inventory = World.Get<InventoryComponent>(player);
    if (inventory.Held is not HeldSlot heldSelection || heldSelection.Index != request.Slot)
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.NotHeld, facts);
      return;
    }

    if (inventory.TryGetHeld() is not ItemStack stack || stack.IsEmpty)
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.EmptySlot, facts);
      return;
    }

    if (!_definitions.TryGet(stack.ItemId, out ItemUse definition))
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.UnknownItem, facts);
      return;
    }

    if (!ItemUseRules.IsReady(tick, state.ReadyTick))
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.NotReady, facts);
      return;
    }

    ItemId ammoItemId = ItemId.None;
    if (definition.RequiresAmmo)
    {
      if (!TryFindAmmo(player, definition, out int ammoSlot))
      {
        Publish(request, state, tick, PlayerItemUseOutcomeKind.MissingAmmo, facts);
        return;
      }

      InventoryTransferResult removal = _inventories.TryRemove(
          player,
          ammoSlot,
          ItemUseRules.AmmoCost(definition),
          out ItemStack removed);
      if (!removal.Succeeded)
      {
        Publish(request, state, tick, PlayerItemUseOutcomeKind.MissingAmmo, facts);
        return;
      }

      ammoItemId = removed.ItemId;
    }

    bool continuous = state.Channeling && definition.Channel;
    SetState(
        player,
        new PlayerItemUseStateComponent(
            request.Slot,
            ItemUseRules.ComputeReadyTick(tick, definition.UseTime),
            tick,
            definition.Channel,
            stack.ItemId,
            ammoItemId),
        pendingStates);
    facts.Enqueue(new PlayerItemUseFact(
        request.Player,
        request.Slot,
        stack.ItemId,
        ammoItemId,
        tick,
        continuous,
        PlayerItemUseOutcomeKind.Applied));
  }

  /// <summary>Finds the first inventory slot holding the required ammunition.</summary>
  private bool TryFindAmmo(ArchEntity player, ItemUse definition, out int ammoSlotIndex)
  {
    ammoSlotIndex = -1;
    if (!World.Has<InventoryComponent>(player))
    {
      return false;
    }

    foreach (InventorySlot candidate in World.Get<InventoryComponent>(player).Slots)
    {
      if (candidate.Content is ItemStack stack &&
          ItemUseRules.IsAmmoFor(definition, stack))
      {
        ammoSlotIndex = candidate.Definition.Index;
        return true;
      }
    }

    return false;
  }

  private static void Publish(
      in PlayerItemUseRequest request,
      in PlayerItemUseStateComponent state,
      long tick,
      PlayerItemUseOutcomeKind outcome,
      PlayerItemUseFactQueueComponent facts)
  {
    facts.Enqueue(new PlayerItemUseFact(
        request.Player,
        request.Slot,
        state.ActiveItem,
        state.ActiveAmmo,
        tick,
        Continuous: false,
        outcome));
  }

  private void SetState(
      ArchEntity player,
      in PlayerItemUseStateComponent state,
      Dictionary<ArchEntity, PlayerItemUseStateComponent> pendingStates)
  {
    if (World.Has<PlayerItemUseStateComponent>(player))
    {
      World.Get<PlayerItemUseStateComponent>(player) = state;
    }
    else
    {
      pendingStates[player] = state;
    }
  }
}
