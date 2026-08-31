using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Items.Snapshots;
using Terraria.Dome.Simulation.Items.Systems;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation;

public sealed partial class DomeSimulation
{
  private readonly Dictionary<int, ShopOfferDefinition> _shopOffers = new();
  private readonly ShopPurchaseState _shopPurchaseState = new();
  private readonly ShopPurchaseSystem _shopPurchaseSystem;
  private readonly ShopOfferCatalogSystem _shopOfferCatalogSystem;
  private readonly NpcInteractionSystem _npcInteractionSystem = new();

  public bool TryGetShopOffer(int offerId, out ShopOfferDefinition offer)
  {
    ThrowIfDisposed();
    return _shopOffers.TryGetValue(offerId, out offer);
  }

  public void RegisterShopOffer(ShopOfferDefinition offer)
  {
    ThrowIfDisposed();
    offer.Validate();
    if (!_itemDefinitions.TryGet(offer.ItemType, out ItemDefinition item) ||
        offer.Quantity > item.StackLimit)
    {
      throw new ArgumentOutOfRangeException(
        nameof(offer),
        "A shop offer must reference an authoritative item Definition and StackLimit.");
    }

    if (!_shopOffers.TryAdd(offer.OfferId, offer))
    {
      throw new ArgumentException(
        "A shop offer with the same offer ID is already registered.",
        nameof(offer));
    }
  }

  public bool TryRefreshGeneratedShopCatalog(
    IReadOnlyCollection<ShopOfferDefinition> generatedOffers)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(generatedOffers);
    List<ShopOfferDefinition> validatedOffers = new(generatedOffers.Count);
    foreach (ShopOfferDefinition offer in generatedOffers)
    {
      try
      {
        offer.Validate();
      }
      catch (ArgumentOutOfRangeException)
      {
        return false;
      }

      if (!_itemDefinitions.TryGet(offer.ItemType, out ItemDefinition item) ||
          offer.Quantity > item.StackLimit)
      {
        return false;
      }

      validatedOffers.Add(offer);
    }

    return _shopOfferCatalogSystem.ReplaceGeneratedOffers(validatedOffers);
  }

  public void QueueNpcInteraction(NpcInteractionCommand command)
  {
    ThrowIfDisposed();
    if (!command.Player.IsValid || !command.Npc.IsValid ||
        !float.IsFinite(command.TargetPosition.X) ||
        !float.IsFinite(command.TargetPosition.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    if (!_players.TryGetValue(command.Player, out Entity playerEntity))
    {
      throw new ArgumentException("Player does not exist.", nameof(command));
    }

    if (!_npcs.TryGetValue(command.Npc, out Entity npcEntity))
    {
      throw new ArgumentException("NPC does not exist.", nameof(command));
    }

    PlayerLifecycleComponent playerLifecycle =
      World.Get<PlayerLifecycleComponent>(playerEntity);
    NpcLifecycleComponent npcLifecycle = World.Get<NpcLifecycleComponent>(npcEntity);
    if (!_npcReplications.TryGetValue(command.Npc, out NpcReplicationSnapshot replication))
    {
      throw new InvalidOperationException("The NPC replication state is unavailable.");
    }

    if (!playerLifecycle.IsActive || !npcLifecycle.IsActive || !replication.IsActive ||
        !World.Has<NpcInteractionComponent>(npcEntity))
    {
      throw new InvalidOperationException(
        "Only active players and NPCs can create an NPC interaction.");
    }

    if (!TryCreateNpcInteraction(
          command,
          playerEntity,
          npcEntity,
          out NpcInteractionCommand accepted))
    {
      throw new InvalidOperationException(
        "The NPC interaction is outside the authoritative town-NPC session boundary.");
    }

    _commands.Enqueue(accepted);
  }

  public void QueueShopPurchase(ShopPurchaseCommand command)
  {
    ThrowIfDisposed();
    if (!command.Player.IsValid || !command.Npc.IsValid || command.OfferId <= 0 ||
        command.Sequence < 0 || string.IsNullOrWhiteSpace(command.SessionId))
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    if (!_players.ContainsKey(command.Player))
    {
      throw new ArgumentException("Player does not exist.", nameof(command));
    }

    if (!_npcs.ContainsKey(command.Npc))
    {
      throw new ArgumentException("NPC does not exist.", nameof(command));
    }

    if (!_shopOffers.ContainsKey(command.OfferId))
    {
      throw new ArgumentException("Shop offer is not registered.", nameof(command));
    }

    _commands.Enqueue(command);
  }

  private bool TryCreateNpcInteraction(
    NpcInteractionCommand command,
    Entity playerEntity,
    Entity npcEntity,
    out NpcInteractionCommand accepted)
  {
    TransformComponent playerTransform = World.Get<TransformComponent>(playerEntity);
    TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
    NpcDefinitionComponent npcComponent = World.Get<NpcDefinitionComponent>(npcEntity);
    PlayerLifecycleComponent playerLifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
    NpcLifecycleComponent npcLifecycle = World.Get<NpcLifecycleComponent>(npcEntity);
    return _npcInteractionSystem.TryCreate(
      command.Player,
      command.Npc,
      command.Kind,
      command.SessionId,
      new SimulationVector(playerTransform.X, playerTransform.Y),
      new SimulationVector(npcTransform.X, npcTransform.Y),
      MaximumInteractionRange,
      playerLifecycle.IsActive,
      npcLifecycle.IsActive,
      out accepted,
      npcComponent.Faction);
  }

  private void CommitNpcInteractions()
  {
    for (int index = 0; index < _commands.NpcInteractionCommands.Count; index++)
    {
      NpcInteractionCommand command = _commands.NpcInteractionCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !_npcs.TryGetValue(command.Npc, out Entity npcEntity) ||
          !World.Has<NpcInteractionComponent>(npcEntity))
      {
        continue;
      }

      PlayerLifecycleComponent playerLifecycle =
        World.Get<PlayerLifecycleComponent>(playerEntity);
      NpcLifecycleComponent npcLifecycle = World.Get<NpcLifecycleComponent>(npcEntity);
      if (!playerLifecycle.IsActive || !npcLifecycle.IsActive)
      {
        continue;
      }

      if (!_npcReplications.TryGetValue(command.Npc, out NpcReplicationSnapshot replication) ||
          !replication.IsActive)
      {
        continue;
      }

      if (!TryCreateNpcInteraction(command, playerEntity, npcEntity,
            out NpcInteractionCommand accepted))
      {
        continue;
      }

      ref NpcInteractionComponent interaction = ref World.Get<NpcInteractionComponent>(npcEntity);
      _npcInteractionSystem.Apply(ref interaction, accepted, TickNumber);
    }
  }

  private void CommitShopPurchases()
  {
    for (int index = 0; index < _commands.ShopPurchaseCommands.Count; index++)
    {
      ShopPurchaseCommand command = _commands.ShopPurchaseCommands[index];
      if (!_shopOffers.TryGetValue(command.OfferId, out ShopOfferDefinition offer))
      {
        continue;
      }

      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !_npcs.TryGetValue(command.Npc, out Entity npcEntity))
      {
        continue;
      }

      if (!World.Has<NpcInteractionComponent>(npcEntity) ||
          !World.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !World.Get<NpcLifecycleComponent>(npcEntity).IsActive)
      {
        continue;
      }

      if (!_npcReplications.TryGetValue(command.Npc, out NpcReplicationSnapshot replication) ||
          !replication.IsActive)
      {
        continue;
      }

      NpcInteractionComponent interaction = World.Get<NpcInteractionComponent>(npcEntity);
      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      ShopPurchaseResult result = _shopPurchaseSystem.TryPurchase(
        command,
        interaction,
        offer,
        inventory,
        _itemDefinitions,
        _shopPurchaseState);
      if (result.IsAccepted)
      {
        PublishInventoryChanges(command.Player, inventory, inventoryBefore);
        if (result.Receipt is ShopPurchaseReceipt receipt)
        {
          _shopOfferCatalogSystem.ApplyPurchaseReceipt(receipt);
          _shopPurchaseReceipts.Add(receipt);
        }
      }
    }
  }
}
