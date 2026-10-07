using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹穿透额度、命中次数和额度耗尽策略。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：penetrate（第 156 行）； maxPenetrate（第 166 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public struct ProjectilePenetrationStateComponent
{
  public ProjectilePenetrationStateComponent(
    int remainingHits = 1,
    int maximumHits = 1,
    int hitCount = 0,
    bool stopsDealingDamageWhenDepleted = false)
  {
    ValidatePenetrationValue(remainingHits, nameof(remainingHits));
    ValidatePenetrationValue(maximumHits, nameof(maximumHits));
    if (hitCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hitCount));
    }

    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    HitCount = hitCount;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public int HitCount;
  public bool StopsDealingDamageWhenDepleted;

  public bool HasRemainingHits => RemainingHits != 0;

  public bool IsUnlimited => RemainingHits == -1;

  private static void ValidatePenetrationValue(int value, string parameterName)
  {
    if (value < -1)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
