using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Entities;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

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
  private readonly ProjectileCatalog _definitions;
  private readonly ProjectileBehaviorCatalog _behaviors;
  private readonly ProjectileRuntimeRegistry _registry;
  private readonly EntitySystem<ArchEntity> _spawner;
  private readonly ProjectileTerminationJournal _journal;
  private ProjectileTerminationQueueComponent? _terminations;

  public ProjectileLifecycleSystem(
      ArchWorld world,
      ProjectileCatalog definitions,
      ProjectileBehaviorCatalog behaviors,
      ProjectileRuntimeRegistry registry,
      ProjectileTerminationJournal? journal = null)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    // 行为目录只为解析 behaviorType：AI 声明必须在实体生成时装配，否则实体存在的第一个
    // tick 没有行为可算，那一帧的速度意图就过期了（设计 D4）。
    _behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    // 终止历史归本域：注册表只做句柄解析，重复终止与陈旧句柄的区分留在这里。
    _journal = journal ?? new ProjectileTerminationJournal(_registry.WorldScope);
    // 生成走共享的实体定义宿主：内容定义在目录里已经和实体组合定义成对登记。
    _spawner = new EntitySystem<ArchEntity>(
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

  public ProjectileSpawnResult TrySpawn(
      in ProjectileSpawnRequest request,
      EcsServerCommandBuffer? commands = null)
  {
    if (!_definitions.TryGet(request.DefinitionId, out ProjectileRecipe recipe))
    {
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.DefinitionUnknown,
          default,
          ArchEntity.Null,
          $"Projectile definition {request.DefinitionId} is not registered.");
    }

    // AI 声明随实体生成即装配（D4）：内容里只有 behavior id / profile id，
    // 实现类型在行为目录上，所以这一步必须解析目录，不能留到适配器补挂。
    if (!recipe.TryGetSection(out ProjectileBasicsSection basics) ||
        !_behaviors.TryGet(basics.BehaviorId, basics.ProfileId, out ProjectileBehaviorProfile behavior))
    {
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.BehaviorUnknown,
          default,
          ArchEntity.Null,
          $"Projectile definition {request.DefinitionId} has no resolvable behavior profile.");
    }

    AiComponent aiDeclaration = AiComponent.For(behavior.BehaviorType);

    if (!_registry.TryResolve(request.Owner, out _))
    {
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.OwnerUnavailable,
          default,
          ArchEntity.Null,
          "The projectile owner is not registered in the current world.");
    }

    if (commands is not null)
    {
      if (commands.IsPlayedBack)
      {
        throw new InvalidOperationException(
            "Projectile creation cannot be deferred after the ECS commit barrier.");
      }

      EntityDefinition deferredDefinition = _definitions.GetEntityDefinition(request.DefinitionId);
      EcsServerEntityCreationTicket ticket =
          _spawner.SpawnDeferred(deferredDefinition.Id, commands);
      ArchEntity bufferedEntity = ticket.BufferedEntity;
      commands.Add(bufferedEntity, new EntityFactionComponent(request.Faction));
      commands.Add(bufferedEntity, new LocationComponent(request.Position));
      commands.Add(bufferedEntity, aiDeclaration);
      commands.Add(bufferedEntity, new ProjectileOwnerComponent(request.Owner));
      commands.Add(bufferedEntity, request.Source);

      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.Deferred,
          ticket.RuntimeHandle!.Value,
          ArchEntity.Null,
          "Projectile creation is pending the current tick commit barrier.",
          ticket);
    }

    EntityDefinition entityDefinition = _definitions.GetEntityDefinition(request.DefinitionId);
    ArchEntity entity = _spawner.Spawn(entityDefinition.Id);
    if (!_registry.TryGetHandle(entity, out EntityRuntimeHandle handle))
    {
      throw new InvalidOperationException("投射物实体创建后未登记运行时句柄。");
    }

    try
    {
      // 实例级输入只剩三样：出生位置、阵营与归属。
      // 速度不在其中（D3）：出厂速度是内容属性，由实体定义装配；之后的每一帧由 AI 行为算。
      // 几何同样不在这里（D6）：碰撞体由内容定义装配，物理系统只消费。
      _world.Add(
          entity,
          new EntityFactionComponent(request.Faction),
          new LocationComponent(request.Position),
          aiDeclaration,
          new ProjectileOwnerComponent(request.Owner),
          request.Source);

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
      if (_journal.Contains(handle))
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
      ProjectileTerminationReason reason,
      EcsServerCommandBuffer? commands = null)
  {
    if (!_registry.TryResolve(handle, out ArchEntity entity))
    {
      return _journal.Contains(handle)
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
    commands?.Destroy(entity);
    if (commands is null)
    {
      _world.Destroy(entity);
    }
    _journal.Record(handle);
    return new ProjectileTerminationResult(
        ProjectileLifecycleStatus.Applied, handle, reason, null);
  }
}
