namespace Terraria.Player;

/// <summary>
/// 保存玩家装备和增益提供的各类弹药节省标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：chloroAmmoCost80（第 1391 行）； huntressAmmoCost90（第 1393 行）； ammoCost80（第 1395 行）；
/// ammoCost75（第 1397 行）； ammoBox（第 1411 行）； ammoPotion（第 1413 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 654 行。</para>
/// <para>
/// 用途：射击流程读取叶绿装备、女猎人装备、弹药箱和弹药药水等来源，再按相应概率规则决定是否消耗弹药。
/// </para>
/// <para>
/// 当前实现：只记录每种来源是否生效。ResetEffects 把所有标记清为 false；概率组合和库存扣减由外部物品使用流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C108，L2（通用可选玩法）。分级理由：随机弹药节省来源不影响固定耗弹的基础武器。
/// </para>
/// </remarks>
public sealed class PlayerAmmoCostPolicyComponent
{
  /// <summary>
  /// 叶绿装备对应的弹药节省规则是否生效。
  /// </summary>
  public bool ChloroAmmoCost80 { get; internal set; }

  /// <summary>
  /// 女猎人装备对应的弹药节省规则是否生效。
  /// </summary>
  public bool HuntressAmmoCost90 { get; internal set; }

  /// <summary>
  /// 原版 ammoCost80 弹药节省来源是否生效。
  /// </summary>
  public bool AmmoCost80 { get; internal set; }

  /// <summary>
  /// 原版 ammoCost75 弹药节省来源是否生效。
  /// </summary>
  public bool AmmoCost75 { get; internal set; }

  /// <summary>
  /// 弹药箱提供的弹药节省效果是否生效。
  /// </summary>
  public bool AmmoBox { get; internal set; }

  /// <summary>
  /// 弹药药水提供的弹药节省效果是否生效。
  /// </summary>
  public bool AmmoPotion { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    ChloroAmmoCost80 = false;
    HuntressAmmoCost90 = false;
    AmmoCost80 = false;
    AmmoCost75 = false;
    AmmoBox = false;
    AmmoPotion = false;
  }
}
