using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Projectile lifecycle metadata over the shared world entity registry.</summary>
/// <remarks>The shared registry owns handle resolution; this type adds termination history.</remarks>
public sealed class ProjectileRuntimeRegistry : IEntityRuntimeResolver, IEntityRuntimeHandleResolver
{
  private readonly HashSet<EntityRuntimeHandle> _terminated = [];

  public ProjectileRuntimeRegistry(long worldScope)
      : this(new EntityRuntimeRegistry(worldScope))
  {
  }

  public ProjectileRuntimeRegistry(EntityRuntimeRegistry entities)
  {
    EntityRegistry = entities ?? throw new ArgumentNullException(nameof(entities));
  }

  public EntityRuntimeRegistry EntityRegistry { get; }

  public long WorldScope => EntityRegistry.WorldScope;

  public int Count => EntityRegistry.Count;

  internal IEnumerable<KeyValuePair<EntityRuntimeHandle, ArchEntity>> Entities =>
      EntityRegistry.Entities;

  public void RegisterOwner(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    EntityRegistry.Register(handle, entity);
  }

  public EntityRuntimeHandle Allocate(EntityUuid id)
  {
    return EntityRegistry.Allocate(id);
  }

  public void RegisterProjectile(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    RegisterOwner(handle, entity);
  }

  public void RegisterEntity(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    RegisterOwner(handle, entity);
  }

  public bool TryResolve(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    return EntityRegistry.TryResolve(handle, out entity);
  }

  public bool TryGetHandle(in ArchEntity entity, out EntityRuntimeHandle handle)
  {
    return EntityRegistry.TryGetHandle(entity, out handle);
  }

  public bool IsTerminated(EntityRuntimeHandle handle)
  {
    return _terminated.Contains(handle);
  }

  public void MarkTerminated(EntityRuntimeHandle handle)
  {
    EnsureWorld(handle);
    _terminated.Add(handle);
  }

  public bool Remove(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    return EntityRegistry.Remove(handle, out entity);
  }

  public void Clear()
  {
    EntityRegistry.Clear();
    _terminated.Clear();
  }

  internal bool SharesRegistryWith(
      IEntityRuntimeResolver resolver,
      IEntityRuntimeHandleResolver handleResolver)
  {
    bool resolverMatches = ReferenceEquals(resolver, this) ||
        ReferenceEquals(resolver, EntityRegistry);
    bool handleResolverMatches = ReferenceEquals(handleResolver, this) ||
        ReferenceEquals(handleResolver, EntityRegistry);
    return resolverMatches && handleResolverMatches;
  }

  private void EnsureWorld(EntityRuntimeHandle handle)
  {
    if (!handle.IsValid || handle.WorldScope != WorldScope)
    {
      throw new ArgumentException("The runtime handle belongs to another world.", nameof(handle));
    }
  }
}
