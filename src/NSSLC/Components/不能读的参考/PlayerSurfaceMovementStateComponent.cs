namespace Terraria.Player;

/// <summary>
/// 保存影响玩家地表移动的沙暴、黏附和滑动标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：sandStorm（第 741 行）； sticky（第 749 行）； slippy（第 751 行）； slippy2（第 753 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 386 行。</para>
/// <para>
/// 用途：地表与环境判定流程写入当前状态，移动流程据此调整滑动、阻力或沙暴相关行为。
/// </para>
/// <para>
/// 当前实现：只保存标记。ResetEffects 和 ResetForLifecycle 都清空四个值，具体方块判断和运动计算由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C146，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerSurfaceMovementStateComponent
{
  /// <summary>
  /// 沙暴相关移动效果是否生效。
  /// </summary>
  public bool SandStorm { get; internal set; }

  /// <summary>
  /// 当前是否受黏性地表或效果影响。
  /// </summary>
  public bool Sticky { get; internal set; }

  /// <summary>
  /// 第一种滑动地表标记。
  /// </summary>
  public bool Slippy { get; internal set; }

  /// <summary>
  /// 第二种滑动地表标记，保留原版 slippy2 的独立规则。
  /// </summary>
  public bool Slippy2 { get; internal set; }

  /// <summary>
  /// 清除沙暴、黏附和两种滑动标记。
  /// </summary>
  internal void ResetEffects()
  {
    SandStorm = false;
    Sticky = false;
    Slippy = false;
    Slippy2 = false;
  }

  /// <summary>
  /// 清除全部地表移动状态标记。
  /// </summary>
  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
