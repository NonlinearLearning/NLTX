using System.Collections.Generic;

namespace Terraria.Items;

public sealed class ContainerLayoutComponent
{
  private readonly List<ContainerSlotRole> _slotRoles;

  public ContainerLayoutComponent(
    IReadOnlyList<ContainerSlotRole> slotRoles,
    SlotIndex? selectedSlot = null,
    long layoutRevision = 0)
  {
    _slotRoles = new List<ContainerSlotRole>(slotRoles);
    SelectedSlot = selectedSlot;
    LayoutRevision = layoutRevision;
  }

  public SlotIndex? SelectedSlot;
  public long LayoutRevision;

  public IReadOnlyList<ContainerSlotRole> SlotRoles => _slotRoles;

  public int SlotCount => _slotRoles.Count;

  public bool HasSelectedSlot =>
    SelectedSlot.HasValue &&
    SelectedSlot.Value.Value >= 0 &&
    SelectedSlot.Value.Value < _slotRoles.Count;
}
