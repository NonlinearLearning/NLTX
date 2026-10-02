using Terraria.Relationships;

namespace Terraria.Items;

public sealed class InventoryComponent
{
  private readonly List<EntityReference> _slots;
  private readonly List<EntityReference> _ammoSlots;
  private readonly List<EntityReference> _coinSlots;

  public InventoryComponent(
    IReadOnlyList<EntityReference> slots,
    int selectedSlot,
    EntityReference trashSlot = default,
    IReadOnlyList<EntityReference>? ammoSlots = null,
    IReadOnlyList<EntityReference>? coinSlots = null,
    long revision = 0)
  {
    _slots = new List<EntityReference>(slots);
    SelectedSlot = selectedSlot;
    TrashSlot = trashSlot;
    _ammoSlots = ammoSlots is null ? [] : new List<EntityReference>(ammoSlots);
    _coinSlots = coinSlots is null ? [] : new List<EntityReference>(coinSlots);
    Revision = revision;
  }

  public int SelectedSlot;
  public EntityReference TrashSlot;
  public long Revision;

  public IReadOnlyList<EntityReference> Slots => _slots;
  public IReadOnlyList<EntityReference> AmmoSlots => _ammoSlots;
  public IReadOnlyList<EntityReference> CoinSlots => _coinSlots;
  public bool HasSelectedSlot => SelectedSlot >= 0 && SelectedSlot < _slots.Count;
  public EntityReference SelectedItem => HasSelectedSlot ? _slots[SelectedSlot] : EntityReference.None;
}
