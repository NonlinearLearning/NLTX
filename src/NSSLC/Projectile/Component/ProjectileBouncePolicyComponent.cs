using System;

namespace Terraria.Unshared.Projectiles;

/// <summary>Declares that a projectile may reflect from tile contacts, and at what cost.</summary>
/// <remarks>
/// 替换原先的 <c>ProjectileBounceCapabilityComponent</c> + <c>ProjectileBounceStateComponent</c>
/// 两件套：那两者在全仓零读取，而"还能反弹几次"与穿透预算共用同一份计数，
/// 不应再维护第二个独立计数器。本组件只承载内容侧策略参数，运行时次数归
/// <c>ProjectilePenetrationStateComponent</c>。
/// </remarks>
public readonly record struct ProjectileBouncePolicyComponent
{
  public ProjectileBouncePolicyComponent(
      int maximumBounces,
      int penetrationCost,
      float speedScale,
      ProjectileBounceConsumeOrder consumeOrder)
  {
    if (maximumBounces < ProjectileBounceSection.Unlimited)
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

    MaximumBounces = maximumBounces;
    PenetrationCost = penetrationCost;
    SpeedScale = speedScale;
    ConsumeOrder = consumeOrder;
  }

  public int MaximumBounces { get; }

  public int PenetrationCost { get; }

  public float SpeedScale { get; }

  public ProjectileBounceConsumeOrder ConsumeOrder { get; }
}
