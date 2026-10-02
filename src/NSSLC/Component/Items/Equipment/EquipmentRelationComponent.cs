using System.Collections.Generic;

namespace Terraria.Items;

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
