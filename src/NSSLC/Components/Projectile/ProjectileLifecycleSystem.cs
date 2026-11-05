using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Relationships;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace Terraria.Projectile;

/// <summary>
/// Owns projectile creation, runtime-handle registration and termination at an exclusive ECS
/// preparation or commit boundary.
/// </summary>
/// <remarks>
/// The host calls this type outside an Arch query iteration. It is deliberately a small owner:
/// definition resolution, identity registration and entity structure live here; movement,
/// collision, target life and network projection remain separate responsibilities.
/// </remarks>
public sealed class ProjectileLifecycleSystem
{
  private readonly ArchWorld _world;
  private readonly ProjectileDefinitionCatalog _definitions;
  private readonly ProjectileRuntimeRegistry _registry;
  private readonly EntityDefinitionSystem<ArchEntity> _spawner;
  private ProjectileTerminationQueueComponent? _terminations;

  public ProjectileLifecycleSystem(
      ArchWorld world,
      ProjectileDefinitionCatalog definitions,
      ProjectileRuntimeRegistry registry)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    // 生成走共享的实体定义宿主：内容定义在目录里已经和实体组合定义成对登记。
    _spawner = new EntityDefinitionSystem<ArchEntity>(
        _definitions.EntityDefinitions,
        new ArchEntityDefinitionHost(_world, _registry.EntityRegistry));
  }

  private ProjectileTerminationQueueComponent Terminations =>
      _terminations ??= ProjectileTerminationQueue.Require(_world);

  /// <summary>Whether this lifecycle shares its runtime registry with the given adapters.</summary>
  public bool SharesRuntimeRegistryWith(
      IEntityRuntimeResolver resolver,
      IEntityRuntimeHandleResolver handleResolver)
  {
    return _registry.SharesRegistryWith(resolver, handleResolver);
  }

  public ProjectileSpawnResult TrySpawn(in ProjectileSpawnRequest request)
  {
    if (!_definitions.TryGet(request.DefinitionId, out ProjectileDefinition? definition))
    {
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.DefinitionUnknown,
          default,
          ArchEntity.Null,
          $"Projectile definition {request.DefinitionId} is not registered.");
    }

    if (!_registry.TryResolve(request.Owner, out _))
    {
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.OwnerUnavailable,
          default,
          ArchEntity.Null,
          "The projectile owner is not registered in the current world.");
    }

    EntityDefinition entityDefinition = _definitions.GetEntityDefinition(request.DefinitionId);
    ArchEntity entity = _spawner.Spawn(entityDefinition.Id);
    if (!_registry.TryGetHandle(entity, out EntityRuntimeHandle handle))
    {
      throw new InvalidOperationException("投射物实体创建后未登记运行时句柄。");
    }

    try
    {
      // 实例级输入：出生位置、速度、阵营、归属、尺寸与碰撞体只有调用方知道；
      // 静态组合（身份、运动学、生命周期、碰撞策略、攻击定义引用、空间前置条件）
      // 已由内容定义配对登记的 EntityDefinition 提供。
      _world.Add(
          entity,
          new EntityFactionComponent(request.Faction),
          request.Size,
          new LocationComponent(request.Position),
          new MovementVelocityComponent(request.Velocity),
          new ProjectileOwnerComponent(request.Owner),
          request.Source,
          request.Collider);

      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.Applied, handle, entity, null);
    }
    catch
    {
      // Creation rollback is structural transaction cleanup; normal termination is committed below.
      _registry.Remove(handle, out _);
      if (_world.IsAlive(entity))
      {
        _world.Destroy(entity);
      }

      throw;
    }
  }

  public ProjectileTerminationResult TryTerminate(
      EntityRuntimeHandle handle,
      ProjectileTerminationReason reason,
      long tick)
  {
    if (!Enum.IsDefined(reason))
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (!_registry.TryResolve(handle, out ArchEntity entity))
    {
      if (_registry.IsTerminated(handle))
      {
        return new ProjectileTerminationResult(
            ProjectileLifecycleStatus.AlreadyTerminated, handle, reason, null);
      }

      return new ProjectileTerminationResult(
          ProjectileLifecycleStatus.StaleReference, handle, reason,
          "The projectile handle is stale or belongs to another world.");
    }

    if (!_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return new ProjectileTerminationResult(
          ProjectileLifecycleStatus.Rejected, handle, reason,
          "The resolved entity does not contain projectile disposition state.");
    }

    ProjectileDispositionStateComponent disposition =
        _world.Get<ProjectileDispositionStateComponent>(entity);
    if (disposition.Disposition == ProjectileDisposition.Terminated)
    {
      return new ProjectileTerminationResult(
          ProjectileLifecycleStatus.AlreadyTerminated, handle, reason, null);
    }

    Terminations.Enqueue(handle, reason, tick);
    return new ProjectileTerminationResult(
        ProjectileLifecycleStatus.Applied, handle, reason, null);
  }

  public int TerminateOwnedBy(
      EntityRuntimeHandle owner,
      ProjectileTerminationReason reason,
      long tick)
  {
    if (!owner.IsValid || !Enum.IsDefined(reason) || tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    EntityRuntimeHandle[] handles = new EntityRuntimeHandle[_registry.Count];
    int count = 0;
    foreach (KeyValuePair<EntityRuntimeHandle, ArchEntity> entry in _registry.Entities)
    {
      EntityRuntimeHandle handle = entry.Key;
      ArchEntity entity = entry.Value;
      if (_world.IsAlive(entity) && _world.Has<ProjectileOwnerComponent>(entity) &&
          _world.Get<ProjectileOwnerComponent>(entity).Owner.Matches(owner))
      {
        handles[count++] = handle;
      }
    }

    int terminated = 0;
    for (int index = 0; index < count; index++)
    {
      if (TryTerminate(handles[index], reason, tick).Succeeded)
      {
        terminated++;
      }
    }

    return terminated;
  }

  internal ProjectileTerminationResult CommitTermination(
      EntityRuntimeHandle handle,
      ProjectileTerminationReason reason)
  {
    if (!_registry.TryResolve(handle, out ArchEntity entity))
    {
      return _registry.IsTerminated(handle)
          ? new ProjectileTerminationResult(
              ProjectileLifecycleStatus.AlreadyTerminated, handle, reason, null)
          : new ProjectileTerminationResult(
              ProjectileLifecycleStatus.StaleReference, handle, reason, null);
    }

    if (!_world.IsAlive(entity) || !_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return new ProjectileTerminationResult(
          ProjectileLifecycleStatus.Rejected, handle, reason, "Projectile entity is unavailable.");
    }

    ref ProjectileDispositionStateComponent disposition =
        ref _world.Get<ProjectileDispositionStateComponent>(entity);
    if (disposition.Disposition == ProjectileDisposition.Terminated)
    {
      return new ProjectileTerminationResult(
          ProjectileLifecycleStatus.AlreadyTerminated, handle, reason, null);
    }

    disposition = new ProjectileDispositionStateComponent(ProjectileDisposition.Terminated);
    _registry.Remove(handle, out _);
    _world.Destroy(entity);
    _registry.MarkTerminated(handle);
    return new ProjectileTerminationResult(
        ProjectileLifecycleStatus.Applied, handle, reason, null);
  }
}
