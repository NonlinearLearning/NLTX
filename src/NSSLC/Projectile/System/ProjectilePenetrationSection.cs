using System;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the penetration family: the spawn-time snapshot of the hit budget.</summary>
/// <remarks>
/// 内容声明的是"最多能穿几个"，运行时要的是"还剩几个"，因此装配点是快照而不是转交：
/// 不在这里快照，第一帧就无处可读。
/// <para>
/// 上限的<b>唯一</b>来源是本类型的 <see cref="MaximumHits" />：内容侧不再把它写进
/// <c>AttributeKind.ProjectilePenetrationLimit</c> 基础值，否则同一件事就有两个真源，
/// 而属性侧那一份运行期根本没人读。
/// </para>
/// </remarks>
public sealed class ProjectilePenetrationSection : IProjectileSection
{
  /// <summary>无限穿透：命中任意数量的目标都不耗尽。</summary>
  public const int Unlimited = -1;

  private readonly int _maximumHits;
  private readonly bool _suppressesDamage;

  public ProjectilePenetrationSection(int maximumHits, bool suppressesDamage = true)
  {
    if (maximumHits < Unlimited)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumHits));
    }

    _maximumHits = maximumHits;
    _suppressesDamage = suppressesDamage;
  }

  /// <summary>可命中的目标数；<see cref="Unlimited" /> 表示无限。</summary>
  public int MaximumHits => _maximumHits;

  public bool IsUnlimited => _maximumHits == Unlimited;

  /// <summary>
  /// 打空后是否停止造成伤害（参考端 <c>stopsDealingDamageAfterPenetrateHits</c>）。
  /// </summary>
  /// <remarks>
  /// 与"还能穿几个"分开：耗尽只表示不能再穿，是否继续造成伤害是另一条内容策略。
  /// </remarks>
  public bool SuppressesDamage => _suppressesDamage;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    // 本族不声明参数块键：预算没有任何跨族读数。
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder.Add(() => ProjectilePenetrationStateComponent.FromLimit(
        _maximumHits,
        _suppressesDamage));
  }
}
