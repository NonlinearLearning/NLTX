using System.Collections.Generic;

namespace Terraria.Items;

public sealed class ContainerContentsComponent
{
  private readonly List<RuntimeEntityId?> _slots;

  public ContainerContentsComponent(
    IReadOnlyList<RuntimeEntityId?> slots,
    long revision = 0,
    long lastMutationTick = 0)
  {
    _slots = new List<RuntimeEntityId?>(slots);
    Revision = revision;
    LastMutationTick = lastMutationTick;
  }

  public long Revision;
  public long LastMutationTick;

  public IReadOnlyList<RuntimeEntityId?> Slots => _slots;

  public int OccupiedSlotCount
  {
    get
    {
      int occupiedSlotCount = 0;

      foreach (RuntimeEntityId? slot in _slots)
      {
        if (slot.HasValue && !slot.Value.IsEmpty)
        {
          occupiedSlotCount++;
        }
      }

      return occupiedSlotCount;
    }
  }

  public bool IsEmpty => OccupiedSlotCount == 0;

  public bool IsFull =>
    _slots.Count > 0 &&
    OccupiedSlotCount == _slots.Count;
}
