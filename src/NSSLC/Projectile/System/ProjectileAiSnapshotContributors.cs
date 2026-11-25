using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

namespace Terraria.Unshared.Projectiles;

/// <summary>Projects one source domain's authoritative values into the projectile AI fact set.</summary>
/// <remarks>
/// 一个 contributor 只管自己那组事实：共享事实（位置、速度）与投射物自己的两项
/// （追踪能力、目标锁）分属不同实现，改动其中一个域不必碰这个类，也不必碰
/// <see cref="ProjectileAiSnapshotProvider" />。
/// <para>
/// 源组件缺失时快照保持原样、不抛异常：绑定侧仍会因为 <see cref="AiRequirements.FindMissing" />
/// 报缺件而拒绝运行，事实缺件与行为需求缺件是同一件事。
/// </para>
/// </remarks>
public interface IProjectileAiSnapshotContributor
{
  /// <summary>Fact types this contributor can project.</summary>
  IReadOnlyList<Type> FactTypes { get; }

  /// <summary>Adds this contributor's facts and returns the resulting snapshot.</summary>
  AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot);
}

/// <summary>Builds the default projectile fact projection set.</summary>
public static class ProjectileAiSnapshotContributors
{
  /// <summary>
  /// Creates one contributor per source domain: position, velocity, homing capability and target.
  /// </summary>
  /// <param name="world">World the facts are read from.</param>
  /// <param name="resolver">Resolver used to turn the locked target handle into an entity.</param>
  public static IReadOnlyList<IProjectileAiSnapshotContributor> CreateDefault(
      ArchWorld world,
      IEntityRuntimeResolver resolver)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(resolver);
    return
    [
      new ProjectileAiLocationSnapshotContributor(world),
      new ProjectileAiVelocitySnapshotContributor(world),
      new ProjectileAiHomingSnapshotContributor(world),
      new ProjectileAiTargetSnapshotContributor(world, resolver)
    ];
  }
}

/// <summary>Projects the projectile's authoritative position.</summary>
internal sealed class ProjectileAiLocationSnapshotContributor : IProjectileAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(LocationComponent)];

  private readonly ArchWorld _world;

  public ProjectileAiLocationSnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    return _world.TryGet(entity, out LocationComponent location)
        ? snapshot.With(location)
        : snapshot;
  }
}

/// <summary>Projects the projectile's authoritative velocity.</summary>
internal sealed class ProjectileAiVelocitySnapshotContributor : IProjectileAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(MovementVelocityComponent)];

  private readonly ArchWorld _world;

  public ProjectileAiVelocitySnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    return _world.TryGet(entity, out MovementVelocityComponent velocity)
        ? snapshot.With(velocity)
        : snapshot;
  }
}

/// <summary>Projects the homing capability this behavior may steer with.</summary>
internal sealed class ProjectileAiHomingSnapshotContributor : IProjectileAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(ProjectileHomingCapabilityComponent)];

  private readonly ArchWorld _world;

  public ProjectileAiHomingSnapshotContributor(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    return _world.TryGet(entity, out ProjectileHomingCapabilityComponent homing)
        ? snapshot.With(homing)
        : snapshot;
  }
}

/// <summary>Projects the locked target into the fact behaviors steer by.</summary>
/// <remarks>
/// 目标事实是"锁"（句柄 + 版本）与目标实体实时位置的合成：锁在、目标还能解析、目标还活着
/// 并且有位置，才写事实。任一条件不成立就不写——行为端因此退化成匀速直线，而不是追击一个
/// 已经消失的目标。
/// </remarks>
internal sealed class ProjectileAiTargetSnapshotContributor : IProjectileAiSnapshotContributor
{
  private static readonly Type[] Types = [typeof(ProjectileTargetFact)];

  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;

  public ProjectileAiTargetSnapshotContributor(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public IReadOnlyList<Type> FactTypes => Types;

  public AiSnapshot Contribute(ArchEntity entity, AiSnapshot snapshot)
  {
    if (!_world.TryGet(entity, out ProjectileTargetComponent lockOn) ||
        !_resolver.TryResolve(lockOn.Target, out ArchEntity target) ||
        !_world.IsAlive(target) ||
        !_world.TryGet(target, out LocationComponent location))
    {
      return snapshot;
    }

    bool alive = !_world.TryGet(target, out LifeComponent life) || life.IsAlive;
    bool visible = !(_world.Has<InvisibilityComponent>(target) &&
        _world.Get<InvisibilityComponent>(target).IsActive);
    return snapshot.With(new ProjectileTargetFact(location.Coordinates, lockOn.Revision, visible, alive));
  }
}
