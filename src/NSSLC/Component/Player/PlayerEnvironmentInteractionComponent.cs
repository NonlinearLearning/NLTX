namespace Terraria.Player;

/// <summary>
/// 保存玩家微光免疫和重力方向。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：shimmerImmune（第 1385 行）； gravDir（第 1389 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 634 行。</para>
/// </remarks>
public sealed class PlayerEnvironmentInteractionComponent
{
  public bool ShimmerImmune { get; internal set; }

  public float GravDir { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    ShimmerImmune = false;
  }

  internal void ResetForLifecycle()
  {
    ShimmerImmune = false;
    GravDir = 1f;
  }
}
