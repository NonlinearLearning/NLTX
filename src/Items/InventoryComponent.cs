using Terraria.Relationships;

namespace Terraria.Items;

public sealed class InventoryComponent
{
  public InventoryComponent(IReadOnlyList<EntityReference> slots, int selectedSlot)
  {
    Slots = new List<EntityReference>(slots);
    SelectedSlot = selectedSlot;
  }

  public List<EntityReference> Slots;
  public int SelectedSlot;
}
