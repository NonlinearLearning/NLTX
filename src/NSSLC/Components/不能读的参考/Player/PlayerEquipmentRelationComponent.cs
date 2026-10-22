namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-708, P09-709, P09-710, P09-711
// crossSubsystemOwner: item payload, equipment effects, and loadout commits remain integration-review
/// <summary>
/// 保存玩家装备槽位与物品实体的关联及装备变更版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armor（第 1013 行）； dye（第 1015 行）； miscEquips（第 1017 行）； miscDyes（第 1019 行）。
/// </para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 124 行。</para>
/// <para>
/// 用途：装备流程根据固定槽位中的 ItemEntityRef 查找物品，再由物品与效果流程读取内容和计算属性。
/// </para>
/// <para>
/// 当前实现：四组数组直接保存引用，Revision 单独记录。物品负载、装备效果和配置切换的提交由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C126，L2（通用可选玩法）。分级理由：装备效果、负载配置和增益属性属于基本持物以外的可选规则。
/// </para>
/// </remarks>
public sealed class PlayerEquipmentRelationComponent
{
  /// <summary>
  /// 装备关联数组容量 20。
  /// </summary>
  public const int ArmorSlotCount = 20;

  /// <summary>
  /// 染料关联数组容量 10。
  /// </summary>
  public const int DyeSlotCount = 10;

  /// <summary>
  /// 杂项装备关联数组容量 5。
  /// </summary>
  public const int MiscEquipmentSlotCount = 5;

  /// <summary>
  /// 杂项染料关联数组容量 5。
  /// </summary>
  public const int MiscDyeSlotCount = 5;

  /// <summary>
  /// 各装备槽位中的物品实体引用。
  /// </summary>
  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  /// <summary>
  /// 各染料槽位中的物品实体引用。
  /// </summary>
  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  /// <summary>
  /// 各杂项装备槽位中的物品实体引用。
  /// </summary>
  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  /// <summary>
  /// 各杂项染料槽位中的物品实体引用。
  /// </summary>
  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];

  /// <summary>
  /// 装备关联的变更版本，由外部提交流程维护。
  /// </summary>
  public long Revision { get; internal set; }
}
