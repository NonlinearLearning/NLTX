using System;

namespace Terraria.Unshared.Projectiles;

/// <summary>穿透预算：弹体自身还能命中几次（相当于弹体的"生命值"）。</summary>
/// <remarks>
/// 三个计数各有分工，缺一不可：
/// <list type="bullet">
/// <item><see cref="Remaining" /> 是<b>已确认</b>的剩余额度，<see cref="Unlimited" /> 表示无限；</item>
/// <item><see cref="PendingConsumed" /> 是本 tick 已入队、等伤害回执的预扣；</item>
/// <item><see cref="Consumed" /> 是累计已确认消耗，供伤害衰减与诊断使用。</item>
/// </list>
/// 为什么需要预扣：Simulation 入队、Commit 才确认，同一 tick 的两次命中会读到同一个
/// <see cref="Consumed" />，"还剩几次"与衰减都会失准。
/// <para>
/// 命中回执必须能回滚：被免疫 / 阵营 / 目标已死拒绝的命中不扣预算，
/// 否则"打空也扣穿透"，与原版 <c>:12635</c> 的 early return 相悖。
/// </para>
/// </remarks>
public readonly record struct ProjectilePenetrationStateComponent
{
  /// <summary>无限穿透的哨兵值，与属性侧的负值约定一致。</summary>
  public const int Unlimited = -1;

  public ProjectilePenetrationStateComponent(
      int remaining,
      int pendingConsumed = 0,
      int consumed = 0,
      bool suppressesDamage = true)
  {
    if (remaining < Unlimited)
    {
      throw new ArgumentOutOfRangeException(nameof(remaining));
    }

    if (pendingConsumed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pendingConsumed));
    }

    if (consumed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(consumed));
    }

    Remaining = remaining;
    PendingConsumed = pendingConsumed;
    Consumed = consumed;
    SuppressesDamage = suppressesDamage;
  }

  /// <summary>已确认的剩余额度；<see cref="Unlimited" /> 表示无限。</summary>
  public int Remaining { get; }

  /// <summary>本 tick 已入队、等回执的预扣数量。</summary>
  public int PendingConsumed { get; }

  /// <summary>累计已确认消耗。</summary>
  public int Consumed { get; }

  /// <summary>
  /// 打空后是否停止造成伤害。
  /// </summary>
  /// <remarks>
  /// 对齐参考端 <c>stopsDealingDamageAfterPenetrateHits</c>：为真时"穿完即哑火"，
  /// 为假时弹体继续飞、继续造成伤害，只是不再穿透。两者都叫"预算 0"，行为相反，
  /// 所以必须显式区分——用一个布尔判断，而不是让调用方各自解释"耗尽"。
  /// </remarks>
  public bool SuppressesDamage { get; }

  public bool IsUnlimited => Remaining == Unlimited;

  public bool IsExhausted => Remaining == 0;

  /// <summary>衰减查表的步数：已确认 + 本 tick 预扣。</summary>
  public int DecaySteps => Consumed + PendingConsumed;

  /// <summary>是否还有额度可以再命中一次（穿透/反弹的判据）。</summary>
  public bool CanConsume => IsUnlimited || Remaining > 0;

  /// <summary>是否还能造成伤害（伤害判据，与穿透判据分离）。</summary>
  public bool CanDealDamage => !IsExhausted || !SuppressesDamage;

  /// <summary>把属性侧上限解析成生成时快照；负值保持无限。</summary>
  public static ProjectilePenetrationStateComponent FromLimit(
      float limit,
      bool suppressesDamage = true)
  {
    if (!float.IsFinite(limit))
    {
      throw new ArgumentOutOfRangeException(nameof(limit));
    }

    if (limit < 0f)
    {
      return new ProjectilePenetrationStateComponent(
          Unlimited,
          suppressesDamage: suppressesDamage);
    }

    return new ProjectilePenetrationStateComponent(
        (int)MathF.Ceiling(limit),
        suppressesDamage: suppressesDamage);
  }

  /// <summary>
  /// 反弹消耗预算：反弹是<b>立即结算</b>的，没有伤害回执要等，所以直接扣剩余额度。
  /// </summary>
  /// <remarks>
  /// 与命中不同：命中要等 Combat 回执才知道算不算数，反弹当场发生就是既成事实。
  /// 用同一份计数而不是第二个计数器，是因为"还能命中几次"和"还能反弹几次"
  /// 在原版里共用同一个 <c>penetrate</c>——两个计数器必然会漂移。
  /// </remarks>
  public ProjectilePenetrationStateComponent ConsumeFor(int cost)
  {
    if (cost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cost));
    }

    int remaining = IsUnlimited ? Unlimited : Math.Max(0, Remaining - cost);
    return new ProjectilePenetrationStateComponent(
        remaining,
        PendingConsumed,
        Consumed + cost,
        SuppressesDamage);
  }

  /// <summary>入队一次命中：先预扣，不动剩余额度。</summary>
  public ProjectilePenetrationStateComponent PreviewConsume()
  {
    return new ProjectilePenetrationStateComponent(
        Remaining,
        PendingConsumed + 1,
        Consumed,
        SuppressesDamage);
  }

  /// <summary>伤害回执为 <c>Applied</c>：扣剩余额度并转成已确认消耗。</summary>
  public ProjectilePenetrationStateComponent ConfirmConsume()
  {
    int remaining = IsUnlimited ? Unlimited : Math.Max(0, Remaining - 1);
    return new ProjectilePenetrationStateComponent(
        remaining,
        Math.Max(0, PendingConsumed - 1),
        Consumed + 1,
        SuppressesDamage);
  }

  /// <summary>伤害回执为 <c>Rejected</c>：只撤预扣，剩余额度不动。</summary>
  public ProjectilePenetrationStateComponent RollbackPending()
  {
    return PendingConsumed == 0
        ? this
        : new ProjectilePenetrationStateComponent(
            Remaining,
            PendingConsumed - 1,
            Consumed,
            SuppressesDamage);
  }
}
