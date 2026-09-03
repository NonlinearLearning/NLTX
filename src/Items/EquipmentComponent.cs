using Terraria.Relationships;

namespace Terraria.Items;

public sealed class EquipmentComponent
{
  public EquipmentComponent(IReadOnlyDictionary<EquipmentSlot, EntityReference> slots)
  {
    Slots = new Dictionary<EquipmentSlot, EntityReference>(slots);
  }

  public Dictionary<EquipmentSlot, EntityReference> Slots;
}
