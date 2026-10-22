namespace Terraria.Player;

/// <summary>
/// 保存玩家常见减益效果的直接布尔标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：cursed（第 1757 行）； bleed（第 1785 行）； confused（第 1787 行）； brokenArmor（第 1795 行）； silence（第
/// 1797 行）； slow（第 1799 行）； gross（第 1801 行）； tongued（第 1803 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 804 行。</para>
/// <para>
/// 用途：状态系统把效果槽位转换成便于移动、战斗和物品使用读取的标记，例如沉默、缓慢或混乱。
/// </para>
/// <para>
/// 当前实现：CommitGross 可单独提交 Gross，ResetEffects 清空全部标记。效果持续时间和具体限制由外部状态流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C119，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerDebuffStatusAliasComponent
{
  /// <summary>
  /// 是否处于诅咒状态。
  /// </summary>
  public bool Cursed { get; internal set; }

  /// <summary>
  /// 是否处于流血状态。
  /// </summary>
  public bool Bleed { get; internal set; }

  /// <summary>
  /// 是否处于混乱状态。
  /// </summary>
  public bool Confused { get; internal set; }

  /// <summary>
  /// 是否处于破甲状态。
  /// </summary>
  public bool BrokenArmor { get; internal set; }

  /// <summary>
  /// 是否处于沉默状态。
  /// </summary>
  public bool Silence { get; internal set; }

  /// <summary>
  /// 是否处于缓慢状态。
  /// </summary>
  public bool Slow { get; internal set; }

  /// <summary>
  /// 原版 gross 效果是否生效。
  /// </summary>
  public bool Gross { get; internal set; }

  /// <summary>
  /// 是否处于舌头牵引状态。
  /// </summary>
  public bool Tongued { get; internal set; }

  /// <summary>
  /// 提交 Gross 标记，不修改其他减益标记。
  /// </summary>
  public void CommitGross(bool gross)
  {
    Gross = gross;
  }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    Cursed = false;
    Bleed = false;
    Confused = false;
    BrokenArmor = false;
    Silence = false;
    Slow = false;
    Gross = false;
    Tongued = false;
  }
}
