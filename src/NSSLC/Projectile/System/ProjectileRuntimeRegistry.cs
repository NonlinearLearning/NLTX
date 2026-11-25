using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Projectile-facing view over the shared world entity registry.</summary>
/// <remarks>
/// 这一层没有自己的状态：句柄分配、绑定、解析与代际全部由共享
/// <see cref="EntityRuntimeRegistry" /> 持有。终止历史不在这里，见
/// <see cref="ProjectileTerminationJournal" />——注册表只回答"这个句柄指向谁"，
/// 不回答"这个句柄是不是已经被终止过"。
/// </remarks>
public sealed class ProjectileRuntimeRegistry : IEntityRuntimeResolver, IEntityRuntimeHandleResolver
{
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

  /// <summary>Binds a handle to its Arch entity; the single registration entry of this domain.</summary>
  /// <remarks>
  /// 没有按角色分名的别名：投射物、发射者、目标都是同一种"句柄→实体"绑定，
  /// 共享注册表也只认一个 <see cref="EntityRuntimeRegistry.Register" />。
  /// </remarks>
  public void Register(EntityRuntimeHandle handle, in ArchEntity entity)
  {
    EntityRegistry.Register(handle, entity);
  }

  public EntityRuntimeHandle Allocate(EntityUuid id)
  {
    return EntityRegistry.Allocate(id);
  }

  public bool TryResolve(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    return EntityRegistry.TryResolve(handle, out entity);
  }

  public bool TryGetHandle(in ArchEntity entity, out EntityRuntimeHandle handle)
  {
    return EntityRegistry.TryGetHandle(entity, out handle);
  }

  public bool Remove(EntityRuntimeHandle handle, out ArchEntity entity)
  {
    return EntityRegistry.Remove(handle, out entity);
  }

  public void Clear()
  {
    EntityRegistry.Clear();
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
}
