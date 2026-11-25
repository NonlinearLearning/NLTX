using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>为"可追踪"的投射物锁定最近的敌对实体。</summary>
/// <remarks>
/// 这是 <see cref="ProjectileTargetFact" /> 的<b>唯一生产者</b>：没有它，追踪行为每帧都因为
/// 快照缺件而退化成匀速直线（原缺陷 B6）。它只写 <see cref="ProjectileTargetComponent" />，
/// 不写速度、不写位置，也不读 AI 输出。
/// <para>
/// 目标筛选与 NPC 侧不同：NPC 只找 <see cref="Faction.Friendly" />，而投射物要找"不是我这边的"，
/// 因为弹体阵营才是敌我判据。归属者自身永远被排除。
/// </para>
/// <para>
/// 结构变更不发生在查询回调里：缺组件时先把"要挂的组件"收进列表，查询结束后统一挂上。
/// 在 Arch 的查询回调里 <c>World.Add</c> 会破坏本次遍历。
/// </para>
/// </remarks>
public sealed class ProjectileTargetAcquisitionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly List<(ArchEntity Entity, EntityRuntimeHandle Target)> _pendingAcquisitions = [];
  private readonly QueryDescription _shooterQuery = new QueryDescription().WithAll<
      ProjectileHomingCapabilityComponent,
      LocationComponent>();
  private readonly QueryDescription _candidateQuery = new QueryDescription().WithAll<
      EntityFactionComponent,
      LifeComponent,
      LocationComponent>();

  public ProjectileTargetAcquisitionSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    _pendingAcquisitions.Clear();
    World.Query(
        in _shooterQuery,
        (ArchEntity entity,
            ref ProjectileHomingCapabilityComponent capability,
            ref LocationComponent location) =>
        {
          if (!capability.Enabled)
          {
            return;
          }

          if (!TryFindNearestHostile(entity, location.Coordinates, capability.Range,
                  out EntityRuntimeHandle target))
          {
            return;
          }

          if (World.TryGet(entity, out ProjectileTargetComponent existing))
          {
            // 已有锁定：同一目标保持版本号，换目标才递增。
            ProjectileTargetComponent retargeted = existing.Retarget(target);
            if (retargeted != existing)
            {
              World.Get<ProjectileTargetComponent>(entity) = retargeted;
            }

            return;
          }

          _pendingAcquisitions.Add((entity, target));
        });

    for (int index = 0; index < _pendingAcquisitions.Count; index++)
    {
      (ArchEntity entity, EntityRuntimeHandle target) = _pendingAcquisitions[index];
      if (World.IsAlive(entity))
      {
        World.Add(entity, new ProjectileTargetComponent(target));
      }
    }

    _pendingAcquisitions.Clear();
  }

  /// <summary>
  /// 找最近的敌对实体；<paramref name="range" /> 为 0 表示不限制距离，与追踪行为的判定口径一致。
  /// </summary>
  private bool TryFindNearestHostile(
      ArchEntity shooter,
      Vector2 origin,
      float range,
      out EntityRuntimeHandle target)
  {
    target = default;
    if (!World.TryGet(shooter, out EntityFactionComponent shooterFaction))
    {
      return false;
    }

    EntityRuntimeHandle owner = World.TryGet(shooter, out ProjectileOwnerComponent ownerComponent)
        ? ownerComponent.Owner
        : default;

    float nearest = range > 0f ? range * range : float.MaxValue;
    EntityRuntimeHandle nearestHandle = default;
    World.Query(
        in _candidateQuery,
        (ArchEntity candidate,
            ref EntityFactionComponent faction,
            ref LifeComponent life,
            ref LocationComponent candidateLocation) =>
        {
          if (candidate == shooter || faction.Value == shooterFaction.Value || !life.IsAlive)
          {
            return;
          }

          if (!_handles.TryGetHandle(candidate, out EntityRuntimeHandle handle) || handle == owner)
          {
            return;
          }

          float distance = Vector2.DistanceSquared(origin, candidateLocation.Coordinates);
          if (distance > nearest)
          {
            return;
          }

          nearest = distance;
          nearestHandle = handle;
        });

    if (!nearestHandle.IsValid)
    {
      return false;
    }

    target = nearestHandle;
    return true;
  }
}
