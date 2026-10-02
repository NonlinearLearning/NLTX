using Terraria.Relationships;

namespace Terraria.Items;

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
