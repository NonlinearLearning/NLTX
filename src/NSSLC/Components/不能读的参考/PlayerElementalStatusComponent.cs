namespace Terraria.Player;

/// <summary>
/// 保存玩家射术、元素效果、视野限制和窒息相关标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：archery（第 1712 行）； poisoned（第 1714 行）； venom（第 1716 行）； blind（第 1718 行）； blackout（第 1720
/// 行）； headcovered（第 1722 行）； frostBurn（第 1724 行）； onFrostBurn（第 1726 行）； onFrostBurn2（第 1728 行）；
/// burned（第 1730 行）； suffocating（第 1740 行）； onFire（第 1749 行）； onFire2（第 1751 行）； onFire3（第 1753
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 704 行。</para>
/// <para>
/// 用途：状态与装备流程汇总当前效果，战斗、环境和表现流程读取燃烧、中毒、视野受限等事实。
/// </para>
/// <para>
/// 当前实现：只记录标记，ResetEffects 全部清为 false。FrostBurn 和 OnFrostBurn 分别保留原版不同字段，具体规则由效果消费流程解释。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C122，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerElementalStatusComponent
{
  /// <summary>
  /// 射术增益是否生效。
  /// </summary>
  public bool Archery { get; internal set; }

  /// <summary>
  /// 是否处于中毒状态。
  /// </summary>
  public bool Poisoned { get; internal set; }

  /// <summary>
  /// 是否处于毒液状态。
  /// </summary>
  public bool Venom { get; internal set; }

  /// <summary>
  /// 是否处于失明状态。
  /// </summary>
  public bool Blind { get; internal set; }

  /// <summary>
  /// 是否处于黑暗视野限制状态。
  /// </summary>
  public bool Blackout { get; internal set; }

  /// <summary>
  /// 是否处于遮头相关状态。
  /// </summary>
  public bool Headcovered { get; internal set; }

  /// <summary>
  /// 原版 frostBurn 效果标记。
  /// </summary>
  public bool FrostBurn { get; internal set; }

  /// <summary>
  /// 第一种霜燃状态是否生效。
  /// </summary>
  public bool OnFrostBurn { get; internal set; }

  /// <summary>
  /// 第二种霜燃状态是否生效。
  /// </summary>
  public bool OnFrostBurn2 { get; internal set; }

  /// <summary>
  /// 是否处于灼烧状态。
  /// </summary>
  public bool Burned { get; internal set; }

  /// <summary>
  /// 是否处于窒息状态。
  /// </summary>
  public bool Suffocating { get; internal set; }

  /// <summary>
  /// 普通燃烧状态是否生效。
  /// </summary>
  public bool OnFire { get; internal set; }

  /// <summary>
  /// 第二种燃烧状态是否生效。
  /// </summary>
  public bool OnFire2 { get; internal set; }

  /// <summary>
  /// 第三种燃烧状态是否生效。
  /// </summary>
  public bool OnFire3 { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    Archery = false;
    Poisoned = false;
    Venom = false;
    Blind = false;
    Blackout = false;
    Headcovered = false;
    FrostBurn = false;
    OnFrostBurn = false;
    OnFrostBurn2 = false;
    Burned = false;
    Suffocating = false;
    OnFire = false;
    OnFire2 = false;
    OnFire3 = false;
  }
}
