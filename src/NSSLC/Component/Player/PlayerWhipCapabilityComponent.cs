namespace Terraria.Player;

/// <summary>
/// 保存玩家鞭子范围和使用时间倍率。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：whipRangeMultiplier（第 769 行）； whipUseTimeMultiplier（第 771 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 436 行。</para>
/// </remarks>
public sealed class PlayerWhipCapabilityComponent
{
  public float WhipRangeMultiplier { get; internal set; } = 1f;

  public float WhipUseTimeMultiplier { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    WhipRangeMultiplier = 1f;
    WhipUseTimeMultiplier = 1f;
  }
}
