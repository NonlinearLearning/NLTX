using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.Relationships;

/// <summary>World-scoped mapping between entity runtime handles and Arch entities.</summary>
/// <remarks>
/// The registry is a lookup boundary, not a second entity state store. It is cleared when the
/// world is unloaded; allocated identities advance their generation before reuse.
/// </remarks>
public sealed class EntityRuntimeRegistry : IEntityRuntimeResolver, IEntityRuntimeHandleResolver
{
  private readonly Dictionary<EntityRuntimeHandle, ArchEntity> _entities = [];
  private readonly Dictionary<ArchEntity, EntityRuntimeHandle> _handles = [];
  private readonly Dictionary<EntityUuid, long> _generations = [];

  public EntityRuntimeRegistry(long worldScope)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    WorldScope = worldScope;
  }

  public long WorldScope { get; }

  public int Count => _entities.Count;

  internal IEnumerable<KeyValuePair<EntityRuntimeHandle, ArchEntity>> Entities => _entities;

  public EntityRuntimeHandle Allocate(EntityUuid id)
  {
    if (!id.IsValid)
    {
      throw new ArgumentException("A valid entity identity is required.", nameof(id));
    }

    long generation = _generations.TryGetValue(id, out long previous)
        ? checked(previous + 1)
        : 1;
    _generations[id] = generation;
    return new EntityRuntimeHandle(id, WorldScope, generation);
  }

  public void Register(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    EnsureWorld(handle);
    if (entity == ArchEntity.Null)
    {
      throw new ArgumentException("The Arch entity must be alive.", nameof(entity));
    }

    if (_entities.ContainsKey(handle) || _handles.ContainsKey(entity))
    {
      throw new InvalidOperationException("The runtime handle or Arch entity is already registered.");
    }

    _entities.Add(handle, entity);
    _handles.Add(entity, handle);
  }

  public void RegisterEntity(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    Register(handle, entity);
  }

  public void RegisterOwner(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    Register(handle, entity);
  }

  public bool TryResolve(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    if (!handle.IsValid || handle.WorldScope != WorldScope)
    {
      entity = ArchEntity.Null;
      return false;
    }

    if (_entities.TryGetValue(handle, out entity) && entity != ArchEntity.Null)
    {
      return true;
    }

    entity = ArchEntity.Null;
    return false;
  }

  public bool TryGetHandle(in ArchEntity entity, out EntityRuntimeHandle handle)
  {
    return _handles.TryGetValue(entity, out handle);
  }

  public bool Remove(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    if (!_entities.Remove(handle, out entity))
    {
      return false;
    }

    _handles.Remove(entity);
    return true;
  }

  public void Clear()
  {
    _entities.Clear();
    _handles.Clear();
  }

  private void EnsureWorld(EntityRuntimeHandle handle)
  {
    if (!handle.IsValid || handle.WorldScope != WorldScope)
    {
      throw new ArgumentException("The runtime handle belongs to another world.", nameof(handle));
    }
  }
}
