using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>
/// 保存功能、时装、染色及隐藏装备槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armor（第 1013 行）； dye（第 1015 行）； miscEquips（第 1017 行）； miscDyes（第 1019 行）；
/// hideVisibleAccessory（第 1208 行）。
/// </para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class EquipmentComponent
{
  private readonly Dictionary<EquipmentSlot, EntityReference> _functionalSlots;
  private readonly Dictionary<EquipmentSlot, EntityReference> _vanitySlots;
  private readonly Dictionary<EquipmentSlot, EntityReference> _dyeSlots;
  private readonly HashSet<EquipmentSlot> _hiddenSlots;

  public EquipmentComponent(
    IReadOnlyDictionary<EquipmentSlot, EntityReference> functionalSlots,
    IReadOnlyDictionary<EquipmentSlot, EntityReference>? vanitySlots = null,
    IReadOnlyDictionary<EquipmentSlot, EntityReference>? dyeSlots = null,
    IReadOnlySet<EquipmentSlot>? hiddenSlots = null,
    long revision = 0)
  {
    _functionalSlots = new Dictionary<EquipmentSlot, EntityReference>(functionalSlots);
    _vanitySlots = vanitySlots is null
      ? []
      : new Dictionary<EquipmentSlot, EntityReference>(vanitySlots);
    _dyeSlots = dyeSlots is null
      ? []
      : new Dictionary<EquipmentSlot, EntityReference>(dyeSlots);
    _hiddenSlots = hiddenSlots is null ? [] : new HashSet<EquipmentSlot>(hiddenSlots);
    Revision = revision;
  }

  public long Revision;

  public IReadOnlyDictionary<EquipmentSlot, EntityReference> FunctionalSlots => _functionalSlots;
  public IReadOnlyDictionary<EquipmentSlot, EntityReference> VanitySlots => _vanitySlots;
  public IReadOnlyDictionary<EquipmentSlot, EntityReference> DyeSlots => _dyeSlots;
  public IReadOnlySet<EquipmentSlot> HiddenSlots => _hiddenSlots;

  // Retained for callers that used the original functional-equipment map.
  public IReadOnlyDictionary<EquipmentSlot, EntityReference> Slots => _functionalSlots;

  public bool IsHidden(EquipmentSlot slot) => _hiddenSlots.Contains(slot);
}
