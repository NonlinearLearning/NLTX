using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>把一个瓦片接触事实翻译成"反弹"或"终止"。</summary>
/// <remarks>
/// 本系统只处理<b>瓦片</b>接触。实体接触归 <see cref="ProjectileDamageRequestSystem" />：
/// 撞墙不产生伤害、撞人不产生反射，两条路径判据完全不同，混在一个系统里必然要按事实类型分支。
/// <para>
/// 判据顺序不是随意选的。<see cref="ProjectileBounceConsumeOrder.BeforeBounce" /> 先看预算、
/// 够了才反弹（脉冲矢：穿完最后一个目标就消失，不会多弹一次）；
/// <see cref="ProjectileBounceConsumeOrder.AfterBounce" /> 先反弹后判死（宝石法杖：最后一次反弹
/// 照做，反弹完才消失）。这两种内容行为在原版里是两套写法，不是一个开关。
/// </para>
/// <para>
/// 速度反射走 <see cref="MovementIntentRequest" />，不直接写 <see cref="VelocityComponent" />：
/// 直写会让"谁有权改速度"多出一个所有者。但意图只是<b>期望速度</b>，
/// <c>SpatialSimulationSystem</c> 还要用 <c>MoveTowards(current, desired, acceleration*dt)</c>
/// 去逼近，而投射物的加速度恒为 0——所以本系统在入队意图的同时把当前速度也写成反射后的值，
/// 让逼近过程退化为"保持不变"。加速度是留给 AI 转向的，不能为反弹破例调大。
/// </para>
/// </remarks>
public sealed class ProjectileTileResolutionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileHitResolutionSystem _projectileHits;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly IEntityRuntimeResolver _resolver;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      ProjectileDispositionStateComponent,
      ProjectileBouncePolicyComponent>();
  private readonly QueryDescription _queueQuery =
      new QueryDescription().WithAll<TileContactQueueComponent>();

  public ProjectileTileResolutionSystem(
      ArchWorld world,
      ProjectileHitResolutionSystem projectileHits,
      IEntityRuntimeHandleResolver handles,
      IEntityRuntimeResolver resolver)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _projectileHits = projectileHits ?? throw new ArgumentNullException(nameof(projectileHits));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  /// <summary>本帧处理结果，供宿主诊断与验证读取。</summary>
  public IReadOnlyList<ProjectileTileResolutionResult> Results => _results;

  private readonly List<ProjectileTileResolutionResult> _results = [];

  public override void Update(in EcsServerTickContext context)
  {
    _results.Clear();
    long tick = context.Tick;
    TileContactQueueComponent queue = DrainQueue();
    if (queue.Count == 0)
    {
      return;
    }

    IReadOnlyList<TileContactFact> facts = queue.Drain();
    ProjectileTerminationQueueComponent terminations = ProjectileTerminationQueue.Require(World);
    MovementIntentRequestQueueComponent movementIntents =
        MovementIntentRequestQueue.Require(World);

    for (int index = 0; index < facts.Count; index++)
    {
      TileContactFact fact = facts[index];
      if (!_resolver.TryResolve(fact.Entity, out ArchEntity entity) ||
          entity == ArchEntity.Null ||
          !World.IsAlive(entity) ||
          !World.Has<ProjectileDispositionStateComponent>(entity) ||
          !World.Has<ProjectileBouncePolicyComponent>(entity))
      {
        continue;
      }

      if (!World.Get<ProjectileDispositionStateComponent>(entity).IsActive)
      {
        continue;
      }

      ProjectileBouncePolicyComponent policy = World.Get<ProjectileBouncePolicyComponent>(entity);
      bool bouncesLeft = policy.MaximumBounces < 0 || policy.MaximumBounces > 0;
      bool budgetAllowsBounce = true;
      if (policy.PenetrationCost > 0 &&
          World.Has<ProjectilePenetrationStateComponent>(entity))
      {
        ProjectilePenetrationStateComponent penetration =
            World.Get<ProjectilePenetrationStateComponent>(entity);
        budgetAllowsBounce = penetration.CanConsume &&
            (penetration.IsUnlimited || penetration.Remaining >= policy.PenetrationCost);
      }

      // BeforeBounce：预算不够就当作硬碰撞，反弹免谈。
      bool bounce = bouncesLeft &&
          (policy.ConsumeOrder == ProjectileBounceConsumeOrder.AfterBounce ||
              budgetAllowsBounce);
      if (!bounce)
      {
        terminations.Enqueue(fact.Entity, ProjectileTerminationReason.TileCollision, tick);
        _results.Add(new ProjectileTileResolutionResult(
            fact.Entity,
            ProjectileTileResolutionOutcome.Terminated,
            Vector2.Zero));
        continue;
      }

      if (policy.PenetrationCost > 0 &&
          World.Has<ProjectilePenetrationStateComponent>(entity))
      {
        ref ProjectilePenetrationStateComponent penetration =
            ref World.Get<ProjectilePenetrationStateComponent>(entity);
        // 反弹与命中共用同一份预算：两者之和达到上限就消失。
        penetration = penetration.ConsumeFor(policy.PenetrationCost);
      }

      Vector2 reflected = Reflect(fact.IncomingVelocity, fact.Geometry.Normal, policy.SpeedScale);
      if (World.Has<VelocityComponent>(entity))
      {
        // 意图只是期望速度，逼近过程需要加速度；投射物加速度恒为 0，
        // 所以这里把当前速度写成反射值，让"逼近"变成"保持"。
        World.Get<VelocityComponent>(entity) = new VelocityComponent(reflected);
      }

      movementIntents.Enqueue(new MovementIntentRequest(
          fact.Entity,
          new MovementIntent(
              reflected,
              tick,
              source: MovementIntentSource.ExternalForce)));

      if (policy.MaximumBounces > 0)
      {
        World.Get<ProjectileBouncePolicyComponent>(entity) = new ProjectileBouncePolicyComponent(
            policy.MaximumBounces - 1,
            policy.PenetrationCost,
            policy.SpeedScale,
            policy.ConsumeOrder);
      }

      // AfterBounce：反弹已经发生，再看预算是否已经打空。
      if (policy.ConsumeOrder == ProjectileBounceConsumeOrder.AfterBounce &&
          policy.PenetrationCost > 0 &&
          World.Has<ProjectilePenetrationStateComponent>(entity) &&
          World.Get<ProjectilePenetrationStateComponent>(entity).IsExhausted)
      {
        terminations.Enqueue(fact.Entity, ProjectileTerminationReason.HitLimitReached, tick);
        _results.Add(new ProjectileTileResolutionResult(
            fact.Entity,
            ProjectileTileResolutionOutcome.BouncedAndExhausted,
            reflected));
        continue;
      }

      _results.Add(new ProjectileTileResolutionResult(
          fact.Entity,
          ProjectileTileResolutionOutcome.Bounced,
          reflected));
    }
  }

  /// <summary>
  /// 供验证直接调用反射公式：反弹方向是内容契约的一部分，
  /// 不该只能靠"跑一帧再看速度"间接观察。
  /// </summary>
  public static Vector2 ReflectForTest(Vector2 incoming, Vector2 normal, float speedScale)
      => Reflect(incoming, normal, speedScale);

  /// <summary>速度沿法线反射并按内容倍率缩放；法线为零时退化为纯缩放。</summary>
  private static Vector2 Reflect(Vector2 incoming, Vector2 normal, float speedScale)
  {
    if (!(normal.LengthSquared() > 0f))
    {
      return incoming * speedScale;
    }

    Vector2 reflected = incoming - 2f * Vector2.Dot(incoming, normal) * normal;
    return reflected * speedScale;
  }

  private TileContactQueueComponent DrainQueue()
  {
    ArchEntity queueEntity = default;
    bool found = false;
    World.Query(
        in _queueQuery,
        (ArchEntity entity) =>
        {
          queueEntity = entity;
          found = true;
        });

    if (!found)
    {
      throw new InvalidOperationException(
          "A TileContactQueueComponent entity must exist before tile contacts are resolved.");
    }

    return World.Get<TileContactQueueComponent>(queueEntity);
  }
}
