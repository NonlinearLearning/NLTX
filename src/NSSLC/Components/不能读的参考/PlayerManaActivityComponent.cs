namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力病效果及活动状态计数。
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
/// <para>
/// 用途：魔法攻击流程读取魔力病和减伤输入，恢复或活动判定流程读取普通与风筝行为相关的 AFK 计数。
/// </para>
/// <para>
/// 状态变化：ResetEffects 只清除魔力病及其减伤输入，保留活动计数；ResetForLifecycle 同时将两个活动计数归零。实际判定和计时由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C133，L2（通用可选玩法）。分级理由：魔力恢复、药水延迟和活动计数只在启用法术／魔力时需要。
/// </para>
/// </remarks>
public sealed class PlayerManaActivityComponent
{
  /// <summary>
  /// 魔力病效果是否生效。
  /// </summary>
  public bool ManaSick { get; internal set; }

  /// <summary>
  /// 魔力病相关伤害降低输入。
  /// </summary>
  public float ManaSickReduction { get; internal set; }

  /// <summary>
  /// 普通活动判定使用的 AFK 计数。
  /// </summary>
  public int AfkCounter { get; internal set; }

  /// <summary>
  /// 风筝行为相关活动判定使用的 AFK 计数。
  /// </summary>
  public int AfkCounterForKiting { get; internal set; }

  /// <summary>
  /// 清除魔力病效果和减伤输入，保留活动计数。
  /// </summary>
  internal void ResetEffects()
  {
    ManaSick = false;
    ManaSickReduction = 0f;
  }

  /// <summary>
  /// 清除魔力病状态，并将两种活动计数归零。
  /// </summary>
  internal void ResetForLifecycle()
  {
    ResetEffects();
    AfkCounter = 0;
    AfkCounterForKiting = 0;
  }
}
