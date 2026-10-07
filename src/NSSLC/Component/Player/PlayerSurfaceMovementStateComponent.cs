namespace Terraria.Player;

/// <summary>
/// 保存玩家沙尘、粘附和滑动表面状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：sandStorm（第 741 行）； sticky（第 749 行）； slippy（第 751 行）； slippy2（第 753 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 386 行。</para>
/// </remarks>
public sealed class PlayerSurfaceMovementStateComponent
{
  public bool SandStorm { get; internal set; }

  public bool Sticky { get; internal set; }

  public bool Slippy { get; internal set; }

  public bool Slippy2 { get; internal set; }

  internal void ResetEffects()
  {
    SandStorm = false;
    Sticky = false;
    Slippy = false;
    Slippy2 = false;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
