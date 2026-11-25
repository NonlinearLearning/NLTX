using System;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the bounce family: reflection policy parameters.</summary>
/// <remarks>
/// 只有声明了反弹的内容才带本 section；"撞瓦片就消失"的内容没有它，
/// 因此不存在"挂着一个 Enabled = false 的反弹能力"这种自相矛盾的中间态。
/// 反弹次数与穿透预算共用同一份计数（<see cref="ProjectilePenetrationStateComponent" />），
/// 本组件只承载内容侧参数。
/// </remarks>
public sealed class ProjectileBounceSection : IProjectileSection
{
  /// <summary>无限反弹：只受寿命约束。</summary>
  public const int Unlimited = -1;

  private readonly int _maximumBounces;
  private readonly int _penetrationCost;
  private readonly float _speedScale;
  private readonly ProjectileBounceConsumeOrder _consumeOrder;

  public ProjectileBounceSection(
      int maximumBounces,
      int penetrationCost,
      float speedScale,
      ProjectileBounceConsumeOrder consumeOrder)
  {
    if (maximumBounces < Unlimited)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumBounces));
    }

    if (penetrationCost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(penetrationCost));
    }

    if (!float.IsFinite(speedScale) || speedScale < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(speedScale));
    }

    if (!Enum.IsDefined(consumeOrder))
    {
      throw new ArgumentOutOfRangeException(nameof(consumeOrder));
    }

    _maximumBounces = maximumBounces;
    _penetrationCost = penetrationCost;
    _speedScale = speedScale;
    _consumeOrder = consumeOrder;
  }

  public int MaximumBounces => _maximumBounces;

  /// <summary>每次反弹消耗多少穿透预算；0 表示不消耗。</summary>
  public int PenetrationCost => _penetrationCost;

  /// <summary>反弹后速度保留的比例。</summary>
  public float SpeedScale => _speedScale;

  public ProjectileBounceConsumeOrder ConsumeOrder => _consumeOrder;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    // 本族不声明参数块键：反弹参数只被自身的策略组件消费。
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder.Add(() => new ProjectileBouncePolicyComponent(
        _maximumBounces,
        _penetrationCost,
        _speedScale,
        _consumeOrder));
  }
}
