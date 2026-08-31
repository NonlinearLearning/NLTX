using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Items.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Items;

public sealed class WorldItemStore
{
  private readonly Dictionary<int, Entity> _entities = new();
  private readonly ArchWorld _world;
  private readonly ItemDefinitionRegistry? _itemDefinitions;

  public WorldItemStore(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public WorldItemStore(ArchWorld world, ItemDefinitionRegistry itemDefinitions)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _itemDefinitions = itemDefinitions ??
      throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public int Count => _entities.Count;

  public int ActiveCount
  {
    get
    {
      int count = 0;
      foreach (Entity entity in _entities.Values)
      {
        if (_world.Get<WorldItemComponent>(entity).IsActive)
        {
          count++;
        }
      }

      return count;
    }
  }

  public IEnumerable<int> Keys => _entities.Keys;

  public IEnumerable<WorldItemComponent> Values
  {
    get
    {
      foreach (Entity entity in _entities.Values)
      {
        yield return _world.Get<WorldItemComponent>(entity);
      }
    }
  }

  public WorldItemComponent this[int replicationId]
  {
    get
    {
      return _world.Get<WorldItemComponent>(_entities[replicationId]);
    }
    set
    {
      Entity entity = _entities[replicationId];
      Synchronize(entity, value);
    }
  }

  public void Add(WorldItemComponent item)
  {
    ValidateItem(item);

    if (_entities.ContainsKey(item.ReplicationId))
    {
      throw new ArgumentException("The world item replication ID is already owned.", nameof(item));
    }

    Entity entity = _world.Create(
      item,
      new ItemStackComponent(item.Stack),
      item.InstanceState,
      item.WorldState,
      new ItemOwnershipComponent(
        null,
        null,
        item.WorldState.SpawnSource,
        item.WorldState.LastOwnerRevision));
    _entities.Add(item.ReplicationId, entity);
  }

  public bool TryGetValue(int replicationId, out WorldItemComponent item)
  {
    if (_entities.TryGetValue(replicationId, out Entity entity))
    {
      item = _world.Get<WorldItemComponent>(entity);
      return true;
    }

    item = default;
    return false;
  }

  public void RecordPickup(int replicationId, PlayerHandle player)
  {
    if (!player.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(player));
    }

    if (!_entities.TryGetValue(replicationId, out Entity entity))
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    WorldItemComponent item = _world.Get<WorldItemComponent>(entity);
    ref ItemOwnershipComponent ownership = ref _world.Get<ItemOwnershipComponent>(entity);
    ownership = ownership with
    {
      Player = player,
      ContainerId = null,
      SourceEntityId = item.WorldState.SpawnSource,
      OwnershipRevision = item.WorldState.LastOwnerRevision
    };
  }

  private void Synchronize(Entity entity, WorldItemComponent item)
  {
    ValidateItem(item);
    ref WorldItemComponent runtimeItem = ref _world.Get<WorldItemComponent>(entity);
    runtimeItem = item;
    ref ItemStackComponent stack = ref _world.Get<ItemStackComponent>(entity);
    stack = new ItemStackComponent(item.Stack);
    ref ItemInstanceStateComponent instanceState =
      ref _world.Get<ItemInstanceStateComponent>(entity);
    instanceState = item.InstanceState;
    ref ItemWorldStateComponent worldState = ref _world.Get<ItemWorldStateComponent>(entity);
    worldState = item.WorldState;
    ref ItemOwnershipComponent ownership = ref _world.Get<ItemOwnershipComponent>(entity);
    ownership = ownership with
    {
      SourceEntityId = item.WorldState.SpawnSource,
      OwnershipRevision = item.WorldState.LastOwnerRevision
    };
  }

  private void ValidateItem(WorldItemComponent item)
  {
    if (item.ReplicationId <= 0 || item.Revision < 0 ||
        !float.IsFinite(item.Position.X) || !float.IsFinite(item.Position.Y) ||
        (item.Stack.IsEmpty && item.Stack != ItemStack.Empty) ||
        item.IsActive != !item.Stack.IsEmpty ||
        item.WorldState.IsActive != item.IsActive ||
        item.WorldState.PickupDelayTicks < 0 || item.WorldState.SpawnSource < 0 ||
        item.WorldState.LastOwnerRevision < 0 || item.WorldState.Revision < 0 ||
        item.WorldState.ReservedPlayerId < 0 ||
        item.WorldState.ReservedPlayerId > ItemWorldStateComponent.UnreservedPlayerId ||
        item.WorldState.ReservationAgeTicks < ItemWorldStateComponent.NoReservationAge)
    {
      throw new ArgumentOutOfRangeException(nameof(item));
    }

    if (_itemDefinitions is not null &&
        !item.Stack.IsEmpty &&
        (!_itemDefinitions.TryGet(item.Stack.ItemType, out ItemDefinition definition) ||
         item.Stack.Quantity > definition.StackLimit))
    {
      throw new ArgumentOutOfRangeException(
        nameof(item),
        "World item stack is unknown or exceeds its definition limit.");
    }

    item.InstanceState.Validate();
    if (item.Stack.IsEmpty && item.InstanceState != default)
    {
      throw new ArgumentException(
        "An inactive world item cannot carry instance state.",
        nameof(item));
    }
  }
}
