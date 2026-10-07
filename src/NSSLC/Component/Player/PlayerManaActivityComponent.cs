namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力病修正和挂机、放风筝计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：manaSickReduction（第 626 行）； manaSick（第 628 行）； afkCounter（第 630 行）；
/// afkCounterForKiting（第 636 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 77 行。</para>
/// </remarks>
public sealed class PlayerManaActivityComponent
{
  public bool ManaSick { get; internal set; }

  public float ManaSickReduction { get; internal set; }

  public int AfkCounter { get; internal set; }

  public int AfkCounterForKiting { get; internal set; }

  internal void ResetEffects()
  {
    ManaSick = false;
    ManaSickReduction = 0f;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
    AfkCounter = 0;
    AfkCounterForKiting = 0;
  }
}
