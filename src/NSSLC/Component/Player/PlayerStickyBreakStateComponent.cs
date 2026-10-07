namespace Terraria.Player;

/// <summary>
/// 保存玩家脱离粘附状态的计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：stickyBreak（第 1399 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 656 行。</para>
/// </remarks>
public sealed class PlayerStickyBreakStateComponent
{
  public int StickyBreak { get; internal set; }

  internal void ResetForLifecycle()
  {
    StickyBreak = 0;
  }
}
