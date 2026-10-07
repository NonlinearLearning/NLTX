using System.Collections.Generic;

namespace Terraria.Items;

/// <summary>
/// 保存装备所属实体及功能、时装、染色和隐藏槽位关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armor（第 1013 行）； dye（第 1015 行）； miscEquips（第 1017 行）； miscDyes（第 1019 行）；
/// hideVisibleAccessory（第 1208 行）。
/// </para>
/// <para>重组说明：OwnerEntity 与 Revision 是装备归属关系和版本的显式表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class EquipmentRelationComponent
{
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _functionalSlots;
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _vanitySlots;
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _dyeSlots;
  private readonly HashSet<EquipmentSlot> _hiddenSlots;

  public EquipmentRelationComponent(
    RuntimeEntityId ownerEntity,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? functionalSlots = null,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? vanitySlots = null,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? dyeSlots = null,
    IReadOnlySet<EquipmentSlot>? hiddenSlots = null,
    long revision = 0)
  {
    OwnerEntity = ownerEntity;
    _functionalSlots = functionalSlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(functionalSlots);
    _vanitySlots = vanitySlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(vanitySlots);
    _dyeSlots = dyeSlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(dyeSlots);
    _hiddenSlots = hiddenSlots is null
      ? []
      : new HashSet<EquipmentSlot>(hiddenSlots);
    Revision = revision;
  }

  public RuntimeEntityId OwnerEntity;
  public long Revision;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> FunctionalSlots =>
    _functionalSlots;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> VanitySlots =>
    _vanitySlots;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> DyeSlots =>
    _dyeSlots;

  public IReadOnlySet<EquipmentSlot> HiddenSlots => _hiddenSlots;

  public bool IsHidden(EquipmentSlot slot) => _hiddenSlots.Contains(slot);
}
