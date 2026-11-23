using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.EntitySpawn;

namespace Terraria.Network;

/// <summary>
/// Binds a freshly spawned creature to a client NPC slot.
/// </summary>
/// <remarks>
/// The NPC domain creates the entity and states its protocol type; the slot is a protocol
/// projection, so it is assigned here rather than inside the spawner. Slots are allocated from the
/// lowest free index in <c>[0, Main.maxNPCs)</c>, which is the same range the client keeps, and the
/// generation increments when a released slot is reused.
/// </remarks>
public sealed class NpcNetworkIdentitySystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  /// <summary>Version4 <c>Main.maxNPCs</c>.</summary>
  public const int MaximumNpcs = 200;

  private readonly QueryDescription _identityQuery =
      new QueryDescription().WithAll<NpcNetworkIdentityComponent>();
  private readonly bool[] _allocated = new bool[MaximumNpcs];
  private readonly byte[] _generations = new byte[MaximumNpcs];
  private readonly ArchEntity[] _owners = new ArchEntity[MaximumNpcs];
  private readonly Dictionary<ArchEntity, NpcNetworkIdentityComponent> _pendingBindings = [];
  private int _allocatedSlotCount;

  public NpcNetworkIdentitySystem(ArchWorld world)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
  }

  public int AllocatedSlotCount => _allocatedSlotCount;

  public bool TryBind(
      ArchEntity entity,
      short netId,
      out NpcNetworkIdentityComponent identity,
      EcsServerCommandBuffer? commands = null)
  {
    identity = default;
    if (entity == ArchEntity.Null || (entity.Id >= 0 && !World.IsAlive(entity)))
    {
      return false;
    }

    bool isBuffered = entity.Id < 0;
    if (isBuffered && (commands is null || commands.IsPlayedBack))
    {
      return false;
    }

    if (_pendingBindings.TryGetValue(entity, out identity))
    {
      if (identity.NetId != netId)
      {
        throw new InvalidOperationException(
            "The entity already has a different pending NPC network type.");
      }

      return true;
    }

    if (!isBuffered && World.Has<NpcNetworkIdentityComponent>(entity))
    {
      identity = World.Get<NpcNetworkIdentityComponent>(entity);
      if (identity.NetId != netId)
      {
        throw new InvalidOperationException(
            "The entity already has a different NPC network type.");
      }

      SynchronizeExistingIdentity(entity, identity);
      return true;
    }

    int slot = FindFreeSlot();
    if (slot < 0)
    {
      return false;
    }

    byte generation = NextGeneration(slot);
    identity = new NpcNetworkIdentityComponent((byte)slot, generation, netId);
    if (commands is null || commands.IsPlayedBack)
    {
      // Synchronous callers (startup and isolated verifiers) retain the legacy adapter path.
      // The server tick registration is Commit-owned and always supplies a writable buffer.
      World.Add(entity, identity);
    }
    else
    {
      commands.Add(entity, identity);
      _pendingBindings.Add(entity, identity);
      int allocatedSlot = slot;
      NpcNetworkIdentityComponent assignedIdentity = identity;
      commands.RegisterAfterPlayback(buffer =>
      {
        _pendingBindings.Remove(entity);
        ArchEntity resolved;
        try
        {
          resolved = buffer.Resolve(entity);
        }
        catch (InvalidOperationException)
        {
          // A create followed by destroy in the same buffer has no live resolution.
          ReleaseAllocatedSlot(entity, assignedIdentity);
          return;
        }

        if (!_allocated[allocatedSlot] || _owners[allocatedSlot] != entity)
        {
          throw new InvalidOperationException(
              "The buffered NPC identity slot was released before playback completed.");
        }

        _owners[allocatedSlot] = resolved;
      });
      commands.RegisterOnDiscard(() =>
      {
        _pendingBindings.Remove(entity);
        ReleaseAllocatedSlot(entity, assignedIdentity);
      });
    }

    _allocated[slot] = true;
    _owners[slot] = entity;
    _allocatedSlotCount++;
    return true;
  }

  public bool TryRelease(ArchEntity entity)
  {
    if (entity == ArchEntity.Null || !World.IsAlive(entity) ||
        !World.Has<NpcNetworkIdentityComponent>(entity))
    {
      return false;
    }

    NpcNetworkIdentityComponent identity =
        World.Get<NpcNetworkIdentityComponent>(entity);
    SynchronizeExistingIdentity(entity, identity);
    int slot = identity.Slot;
    if (!_allocated[slot] || _owners[slot] != entity ||
        _generations[slot] != identity.Generation)
    {
      return false;
    }

    _allocated[slot] = false;
    _owners[slot] = ArchEntity.Null;
    _allocatedSlotCount--;
    return true;
  }

  public void Clear()
  {
    Array.Clear(_allocated);
    Array.Clear(_generations);
    Array.Clear(_owners);
    _allocatedSlotCount = 0;
  }

  public override void Update(in EcsServerTickContext context)
  {
    _pendingBindings.Clear();
    SynchronizeExistingIdentities();
    EntitySpawnFactQueueComponent queue = EntitySpawnFactQueue.Require(World);
    IReadOnlyList<EntitySpawnedFact> facts = queue.Drain();
    if (facts.Count == 0)
    {
      return;
    }

    for (int index = 0; index < facts.Count; index++)
    {
      EntitySpawnedFact fact = facts[index];
      if (!IsAvailableEntity(fact.Entity))
      {
        continue;
      }

      if (!TryBind(fact.Entity, fact.NetworkType, out _, context.Commands))
      {
        if (fact.Entity.Id < 0 && context.Commands is not null)
        {
          // A buffered NPC cannot be deferred to a later tick. If no protocol slot is available,
          // cancel the creation before Commit playback so an authoritative entity never exists
          // without the identity required by Projection.
          context.Commands.Destroy(fact.Entity);
          continue;
        }

        List<EntitySpawnedFact> remainder = [];
        for (int remainderIndex = index; remainderIndex < facts.Count; remainderIndex++)
        {
          EntitySpawnedFact pending = facts[remainderIndex];
          // A buffered entity belongs to this tick's command buffer and cannot be carried into
          // the next tick. Live entities can safely wait for a free protocol slot.
          if (pending.Entity.Id >= 0 && IsAvailableEntity(pending.Entity))
          {
            remainder.Add(pending);
          }
        }

        queue.Requeue(remainder);
        return;
      }
    }
  }

  private bool IsAvailableEntity(ArchEntity entity)
  {
    return entity != ArchEntity.Null && (entity.Id < 0 || World.IsAlive(entity));
  }

  private void ReleaseAllocatedSlot(
      ArchEntity entity,
      NpcNetworkIdentityComponent identity)
  {
    int slot = identity.Slot;
    if (slot >= MaximumNpcs || !_allocated[slot] ||
        (_owners[slot] != entity && _owners[slot] != ArchEntity.Null) ||
        _generations[slot] != identity.Generation)
    {
      return;
    }

    _allocated[slot] = false;
    _owners[slot] = ArchEntity.Null;
    _allocatedSlotCount--;
  }

  private void SynchronizeExistingIdentities()
  {
    World.Query(
        in _identityQuery,
        (ArchEntity entity, ref NpcNetworkIdentityComponent identity) =>
            SynchronizeExistingIdentity(entity, identity));
  }

  private void SynchronizeExistingIdentity(
      ArchEntity entity,
      NpcNetworkIdentityComponent identity)
  {
    if (identity.Generation == 0 || identity.Slot >= MaximumNpcs)
    {
      throw new InvalidOperationException(
          "The NPC network identity has an invalid slot or generation.");
    }

    int slot = identity.Slot;
    if (_allocated[slot])
    {
      if (_owners[slot] != entity || _generations[slot] != identity.Generation)
      {
        throw new InvalidOperationException(
            "Two live NPC entities share a network slot or generation.");
      }

      return;
    }

    _allocated[slot] = true;
    _owners[slot] = entity;
    _generations[slot] = identity.Generation;
    _allocatedSlotCount++;
  }

  private int FindFreeSlot()
  {
    for (int slot = 0; slot < MaximumNpcs; slot++)
    {
      if (!_allocated[slot])
      {
        return slot;
      }
    }

    return -1;
  }

  private byte NextGeneration(int slot)
  {
    byte previous = _generations[slot];
    byte next = previous == byte.MaxValue
        ? (byte)1
        : checked((byte)(previous + 1));
    _generations[slot] = next;
    return next;
  }
}
