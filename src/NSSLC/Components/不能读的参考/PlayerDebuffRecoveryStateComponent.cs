namespace Terraria.Player;

/// <summary>
/// 保存玩家控制减益、生命恢复加成和幽灵装备效果标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：chilled（第 656 行）； dazed（第 658 行）； frozen（第 660 行）； stoned（第 662 行）； ichor（第 664 行）；
/// webbed（第 666 行）； tipsy（第 668 行）； noBuilding（第 670 行）； crimsonRegen（第 743 行）； ghostHeal（第 745
/// 行）； ghostHurt（第 747 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 78 行。</para>
/// <para>
/// 用途：状态与装备流程汇总冰冻、石化、禁建等控制状态及恢复效果，移动、交互和战斗流程读取这些结果。
/// </para>
/// <para>
/// 当前实现：只记录效果是否生效，默认 false。ResetEffects 和 ResetForLifecycle 都清空全部标记；控制行为和恢复量由外部系统决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C118，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerDebuffRecoveryStateComponent
{
  /// <summary>
  /// 是否处于寒冷状态。
  /// </summary>
  public bool Chilled { get; internal set; }

  /// <summary>
  /// 是否处于眩晕状态。
  /// </summary>
  public bool Dazed { get; internal set; }

  /// <summary>
  /// 是否处于冰冻状态。
  /// </summary>
  public bool Frozen { get; internal set; }

  /// <summary>
  /// 是否处于石化状态。
  /// </summary>
  public bool Stoned { get; internal set; }

  /// <summary>
  /// 是否受灵液效果影响。
  /// </summary>
  public bool Ichor { get; internal set; }

  /// <summary>
  /// 是否处于蛛网束缚状态。
  /// </summary>
  public bool Webbed { get; internal set; }

  /// <summary>
  /// 是否处于微醺状态。
  /// </summary>
  public bool Tipsy { get; internal set; }

  /// <summary>
  /// 是否禁止建造。
  /// </summary>
  public bool NoBuilding { get; internal set; }

  /// <summary>
  /// 是否启用原版 crimsonRegen 恢复效果。
  /// </summary>
  public bool CrimsonRegen { get; internal set; }

  /// <summary>
  /// 是否启用幽灵装备的治疗效果。
  /// </summary>
  public bool GhostHeal { get; internal set; }

  /// <summary>
  /// 是否启用幽灵装备的伤害效果。
  /// </summary>
  public bool GhostHurt { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    Chilled = false;
    Dazed = false;
    Frozen = false;
    Stoned = false;
    Ichor = false;
    Webbed = false;
    Tipsy = false;
    NoBuilding = false;
    CrimsonRegen = false;
    GhostHeal = false;
    GhostHurt = false;
  }

  /// <summary>
  /// 清空全部效果标记，用于初始化、重生或复用。
  /// </summary>
  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
