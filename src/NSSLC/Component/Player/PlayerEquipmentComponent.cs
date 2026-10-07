namespace Terraria.Player;

/// <summary>
/// 保存玩家装备、染色、杂项和隐藏槽位及当前配置。
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
/// </remarks>
public sealed class PlayerEquipmentComponent
{
  public const int ArmorSlotCount = 20;
  public const int DyeSlotCount = 10;
  public const int MiscEquipmentSlotCount = 5;
  public const int MiscDyeSlotCount = 5;
  public const int HiddenAccessorySlotCount = 10;

  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];

  public bool[] HiddenAccessorySlots { get; } =
    new bool[HiddenAccessorySlotCount];

  public int CurrentLoadoutIndex { get; set; }
}
