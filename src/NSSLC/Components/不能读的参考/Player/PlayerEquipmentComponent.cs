namespace Terraria.Player;

/// <summary>
/// 保存玩家装备、染料、杂项装备槽位和饰品显示选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armor（第 1013 行）； dye（第 1015 行）； miscEquips（第 1017 行）； miscDyes（第 1019 行）；
/// hideVisibleAccessory（第 1208 行）； CurrentLoadoutIndex（第 2491 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 54 行。</para>
/// <para>
/// 用途：装备流程从槽位读取物品实体，汇总装备效果；表现流程读取染料和隐藏饰品标记。
/// </para>
/// <para>
/// 当前实现：数组长度固定，数组内容可修改。CurrentLoadoutIndex 只保存选择值；装备切换、物品转移和效果计算由外部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C125，L2（通用可选玩法）。分级理由：装备效果、负载配置和增益属性属于基本持物以外的可选规则。
/// </para>
/// </remarks>
public sealed class PlayerEquipmentComponent
{
  /// <summary>
  /// 护甲、饰品等原版装备数组容量 20。
  /// </summary>
  public const int ArmorSlotCount = 20;
  /// <summary>
  /// 装备染料槽位容量 10。
  /// </summary>
  public const int DyeSlotCount = 10;
  /// <summary>
  /// 杂项装备槽位容量 5。
  /// </summary>
  public const int MiscEquipmentSlotCount = 5;
  /// <summary>
  /// 杂项装备染料槽位容量 5。
  /// </summary>
  public const int MiscDyeSlotCount = 5;
  /// <summary>
  /// 饰品隐藏标记容量 10。
  /// </summary>
  public const int HiddenAccessorySlotCount = 10;

  /// <summary>
  /// 装备槽位中的物品实体引用数组。
  /// </summary>
  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  /// <summary>
  /// 装备染料槽位中的物品实体引用数组。
  /// </summary>
  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  /// <summary>
  /// 杂项装备物品实体引用数组。
  /// </summary>
  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  /// <summary>
  /// 杂项装备染料物品实体引用数组。
  /// </summary>
  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];

  /// <summary>
  /// 各饰品槽位是否隐藏外观的标记数组。
  /// </summary>
  public bool[] HiddenAccessorySlots { get; } =
    new bool[HiddenAccessorySlotCount];

  /// <summary>
  /// 当前选中的装备配置索引，默认 0。
  /// </summary>
  public int CurrentLoadoutIndex { get; set; }
}
