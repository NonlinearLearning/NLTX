namespace Terraria.Player;

/// <summary>
/// 保存玩家幽灵模式、动画和 PvP 死亡标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：ghost（第 910 行）； ghostFrame（第 912 行）； ghostFrameCounter（第 914 行）； pvpDeath（第 921 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 476 行。</para>
/// </remarks>
public sealed class PlayerGhostStateComponent
{
  public bool Ghost { get; internal set; }

  public int GhostFrame { get; internal set; }

  public int GhostFrameCounter { get; internal set; }

  public bool PvpDeath { get; internal set; }

  public void CommitGhost(bool ghost)
  {
    Ghost = ghost;
  }

  internal void ResetForLifecycle()
  {
    Ghost = false;
    GhostFrame = 0;
    GhostFrameCounter = 0;
    PvpDeath = false;
  }
}
