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
  private ProjectileTerminationQueueComponent? _terminations;

  public ProjectileLifecycleSystem(
      ArchWorld world,
      ProjectileDefinitionCatalog definitions,
      ProjectileRuntimeRegistry registry)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
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

    EntityUuid id = EntityUuid.Create();
    EntityRuntimeHandle handle = _registry.Allocate(id);
    EntityIdentityComponent identity = new(id);
    EntityFactionComponent faction = new(request.Faction);
    LocationComponent location = new(request.Position);
    MovementVelocityComponent velocity = new(request.Velocity);
    ProjectileOwnerComponent owner = new(request.Owner);
    ProjectileDispositionStateComponent disposition =
        new(ProjectileDisposition.Active);
    ArchEntity entity = _world.Create(
        identity,
        faction,
        request.Size,
        location,
        velocity,
        request.Collider,
        definition.Collision,
        owner,
        request.Source,
        definition.Behavior,
        new ProjectileBehaviorStateComponent(0),
        definition.Kinematics,
        definition.Lifetime,
        new ProjectilePenetrationStateComponent(),
        definition.Attributes.Clone(),
        new ProjectileHitImmunityStateComponent(),
        definition.CollisionPolicy,
        new ProjectileAttackDefinitionReferenceComponent(definition.AttackDefinitionId),
        disposition);

    try
    {
      // 空间运动组件分两步添加：Arch 的泛型 Create 有类型参数上限，原来 19 项就已接近，
      // 再塞 9 项会超出上限，编译器退化到 ComponentType 重载后全盘报错。
      //
      // 这一组是共享 SpatialSimulationCoordinatorSystem 的前置条件：查询要求
      // Location/Velocity/Collider/Grounding，回调里再 TryGet
      // MovementCapability/GravityCapability/GravityState/TileCollisionCapability/
      // LiquidInteractionCapability/MovementIntent。缺任何一项都会被静默跳过，
      // 投射物就永远不动。
      _world.Add(
          entity,
          GroundingComponent.Airborne,
          // acceleration 必须为 0：MovementIntent.Stop 的 HasDesiredVelocity 为 true、
          // 目标速度为零，非零加速度会让 MoveTowards 把投射物逐帧减速到静止。
          // maximumSpeed 取 0 表示不钳制（SpatialSimulationSystem 只在 >0 时生效）。
          new MovementCapabilityComponent(
              enabled: true,
              maximumSpeed: 0f,
              acceleration: 0f,
              canJump: false,
              jumpSpeed: 0f,
              mode: MovementMode.Flight),
          // Flight 模式跳过重力积分，因此投射物按出生速度匀速直线飞行。
          // 逐类型重力需要 ProjectileDefinition 增加内容字段，届时再从定义取值；
          // 现在定义里没有重力，硬编码一个有值才是凭空造内容。
          new GravityCapabilityComponent(Enabled: false, CanToggleDirection: false),
          new GravityConfigurationComponent(),
          new GravityDirectionStateComponent(GravityDirection.Normal),
          new GravityModifierCollectionComponent(),
          new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false),
          new TileCollisionCapabilityComponent(Enabled: true),
          new LiquidInteractionCapabilityComponent(
              Enabled: true,
              AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
                  LiquidContactFlags.Lava | LiquidContactFlags.Shimmer),
          MovementIntent.Stop(tick: 0));

      _registry.RegisterProjectile(handle, entity);
      return new ProjectileSpawnResult(
          ProjectileLifecycleStatus.Applied, handle, entity, null);
    }
    catch
    {
      // Creation rollback is structural transaction cleanup; normal termination is committed below.
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
