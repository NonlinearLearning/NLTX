namespace Terraria.Projectile;

/// <summary>
/// 保存射弹的各类液体接触状态和进出液体冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； wetCount（第 32 行）； lavaWet（第 34 行）。
/// </para>
/// <para>拆分依据目录：docs/system-decomposition/reports/。</para>
/// <para>
/// 拆分依据文件：2026-09-30-system-decomposition-authoritative-P15-projectile-execution.md。
/// </para>
/// <para>依据位置：第 789 行。</para>
/// </remarks>
public struct ProjectileWetStateComponent
{
  public const int WetTransitionCooldownTicks = 10;

  public bool Wet;
  public bool LavaWet;
  public bool HoneyWet;
  public bool ShimmerWet;
  public int WetCount;
}
