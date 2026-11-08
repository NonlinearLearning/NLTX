using System;
using System.Collections.Generic;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>
/// Applies item use requests, ported from the gate and ammo part of Version4 <c>Player.ItemCheck</c>.
/// </summary>
/// <remarks>
/// Per request the system replays the reference's order: does the slot hold a usable stack, is the
/// use gate open, is the required ammunition present, then commit the gate, the consumed ammo and
/// the channel flag and publish one fact. Mana spend is not applied: the ECS has no mana resource
/// yet, so <see cref="ItemUseDefinition.ManaCost"/> travels as data only and the gap is recorded
/// rather than guessed at.
/// </remarks>
public sealed class PlayerItemUseSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly ItemUseDefinitionCatalog _definitions;
  private readonly InventorySystem _inventories;
  private PlayerItemUseFactQueueComponent? _facts;

  public PlayerItemUseSystem(
      ArchWorld ecsWorld,
      IEntityRuntimeResolver resolver,
      ItemUseDefinitionCatalog definitions,
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
    for (int index = 0; index < requests.Count; index++)
    {
      Apply(requests[index], context.Tick, facts);
    }
  }

  private void Apply(
      in PlayerItemUseRequest request,
      long tick,
      PlayerItemUseFactQueueComponent facts)
  {
    if (!_resolver.TryResolve(request.Player, out ArchEntity player) ||
        player == ArchEntity.Null || !World.IsAlive(player))
    {
      return;
    }

    // 状态组件按需挂载：玩家实体由 Communication 创建，使用门闸属于 Player 域的附加状态。
    if (!World.Has<PlayerItemUseStateComponent>(player))
    {
      World.Add(player, PlayerItemUseStateComponent.Idle);
    }

    PlayerItemUseStateComponent state = World.Get<PlayerItemUseStateComponent>(player);
    if (request.Kind == PlayerItemUseRequestKind.ReleaseUse)
    {
      Release(request, state, player, tick, facts);
      return;
    }

    Begin(request, state, player, tick, facts);
  }

  private void Release(
      in PlayerItemUseRequest request,
      in PlayerItemUseStateComponent state,
      ArchEntity player,
      long tick,
      PlayerItemUseFactQueueComponent facts)
  {
    bool wasChanneling = state.Channeling;
    World.Get<PlayerItemUseStateComponent>(player) = state with { Channeling = false };
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
      PlayerItemUseFactQueueComponent facts)
  {
    if (!World.Has<InventoryComponent>(player))
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.NoInventory, facts);
      return;
    }

    InventoryComponent inventory = World.Get<InventoryComponent>(player);
    if (!inventory.TryGetSlot(request.Slot, out InventorySlot slot) ||
        slot.Content is not ItemStack stack ||
        stack.IsEmpty)
    {
      Publish(request, state, tick, PlayerItemUseOutcomeKind.EmptySlot, facts);
      return;
    }

    if (!_definitions.TryGet(stack.ItemId, out ItemUseDefinition definition))
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
    World.Get<PlayerItemUseStateComponent>(player) = new PlayerItemUseStateComponent(
        request.Slot,
        ItemUseRules.ComputeReadyTick(tick, definition.UseTime),
        tick,
        definition.Channel,
        stack.ItemId,
        ammoItemId);
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
  private bool TryFindAmmo(ArchEntity player, ItemUseDefinition definition, out int ammoSlotIndex)
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
}
