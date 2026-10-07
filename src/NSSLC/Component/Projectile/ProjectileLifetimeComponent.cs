namespace Terraria.Projectile;

/// <summary>
/// 保存射弹剩余寿命、活动状态和终止原因。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：active（第 90 行）； timeLeft（第 138 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spawn-lifecycle-and-loot-component-design.md。</para>
/// <para>依据位置：第 141 行。</para>
/// </remarks>
public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent()
  {
    RemainingTicks = 3600;
    EndReason = ProjectileEndReason.None;
  }

  public ProjectileLifetimeComponent(
    int remainingTicks,
    ProjectileEndReason endReason = ProjectileEndReason.None)
  {
    RemainingTicks = remainingTicks;
    EndReason = endReason;
  }

  public int RemainingTicks;
  public ProjectileEndReason EndReason;

  public bool IsActive => RemainingTicks > 0;

  public bool IsExpired => RemainingTicks == 0;
}
