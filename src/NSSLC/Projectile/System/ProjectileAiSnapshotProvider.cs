using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

namespace Terraria.Unshared.Projectiles;

/// <summary>把投射物的权威 ECS 值投影成行为可读的不可变事实。</summary>
/// <remarks>
/// 行为只看到事实，看不到 <see cref="ArchWorld" />：这让"追踪怎么算"与"数据从哪来"分开，
/// 也让行为可以在没有世界的验证项目里直接跑。
/// <para>
/// 目标事实由 <see cref="ProjectileTargetComponent" />（句柄 + 版本）加目标实体的实时位置合成；
/// 目标丢失、死亡或不可见时不写入该事实，行为端会退化成匀速直线，而不是追击一个幻影。
/// </para>
/// </remarks>
public sealed class ProjectileAiSnapshotProvider : IAiSnapshotProvider<ArchEntity>
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;

  public ProjectileAiSnapshotProvider(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public AiSnapshot Capture(ArchEntity entity, IReadOnlyList<Type> requiredComponents)
  {
    ArgumentNullException.ThrowIfNull(requiredComponents);
    AiSnapshot snapshot = AiSnapshot.Empty;

    bool needsLocation = requiredComponents.Contains(typeof(LocationComponent));
    bool needsVelocity = requiredComponents.Contains(typeof(MovementVelocityComponent));
    bool needsHoming = requiredComponents.Contains(typeof(ProjectileHomingCapabilityComponent));
    bool needsTarget = requiredComponents.Contains(typeof(ProjectileTargetFact));

    if (needsLocation && _world.TryGet(entity, out LocationComponent location))
    {
      snapshot = snapshot.With(location);
    }

    if (needsVelocity && _world.TryGet(entity, out MovementVelocityComponent velocity))
    {
      snapshot = snapshot.With(velocity);
    }

    if (needsHoming && _world.TryGet(entity, out ProjectileHomingCapabilityComponent homing))
    {
      snapshot = snapshot.With(homing);
    }

    if (needsTarget && TryCaptureTarget(entity, out ProjectileTargetFact target))
    {
      snapshot = snapshot.With(target);
    }

    return snapshot;
  }

  private bool TryCaptureTarget(ArchEntity projectile, out ProjectileTargetFact fact)
  {
    fact = default;
    if (!_world.TryGet(projectile, out ProjectileTargetComponent lockOn) ||
        !_resolver.TryResolve(lockOn.Target, out ArchEntity target) ||
        !_world.IsAlive(target) ||
        !_world.TryGet(target, out LocationComponent location))
    {
      return false;
    }

    bool alive = !_world.TryGet(target, out Terraria.Share.Combat.LifeComponent life) || life.IsAlive;
    bool visible = !(_world.Has<InvisibilityComponent>(target) &&
        _world.Get<InvisibilityComponent>(target).IsActive);
    fact = new ProjectileTargetFact(location.Coordinates, lockOn.Revision, visible, alive);
    return true;
  }
}
