using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹活动标记、剩余时间和终止原因。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：active（第 90 行）； timeLeft（第 138 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileLifetimeStateComponent
{
  public const int DefaultTimeLeft = 3600;

  public ProjectileLifetimeStateComponent(
    bool active = true,
    int timeLeft = DefaultTimeLeft,
    ProjectileEndReason endReason = ProjectileEndReason.None)
  {
    if (timeLeft < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(timeLeft));
    }

    if (endReason == ProjectileEndReason.WorldBoundary && active)
    {
      throw new ArgumentException(
        "A world-boundary deactivated projectile must be inactive.",
        nameof(active));
    }

    if (endReason != ProjectileEndReason.None &&
      endReason != ProjectileEndReason.WorldBoundary &&
      (active || timeLeft != 0))
    {
      throw new ArgumentException(
        "A terminated projectile must be inactive with no remaining time.",
        nameof(endReason));
    }

    Active = active;
    TimeLeft = timeLeft;
    EndReason = endReason;
  }

  public bool Active;

  public int TimeLeft;

  public ProjectileEndReason EndReason;

  public bool IsActive => Active;

  public bool IsExpired => TimeLeft == 0;
}
