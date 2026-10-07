namespace Terraria.Player;

/// <summary>
/// 保存玩家微光转化、透明度和脱困保护状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：shimmering（第 1732 行）； timeShimmering（第 1734 行）； shimmerTransparency（第 1736 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 706 行。</para>
/// </remarks>
public sealed class PlayerShimmerStateComponent
{
  public bool Shimmering { get; internal set; }

  public int TimeShimmering { get; internal set; }

  public float ShimmerTransparency { get; internal set; }

  // Stable state extracted from Version4 ShimmerUnstuckHelper; adapter behavior remains external.
  public int ShimmerUnstuckTimeLeft { get; internal set; }

  public bool ShimmerUnstuckProtectionActive { get; internal set; }

  internal void ResetForLifecycle()
  {
    Shimmering = false;
    TimeShimmering = 0;
    ShimmerTransparency = 0f;
    ShimmerUnstuckTimeLeft = 0;
    ShimmerUnstuckProtectionActive = false;
  }
}
