namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldItemSlotStore
{
  private readonly Slot[] _slots;
  private int _nextRuntimeEntityId;

  public WorldItemSlotStore(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = Enumerable.Range(0, capacity).Select(_ => new Slot()).ToArray();
  }

  public int Capacity => _slots.Length;

  public bool TryAllocate(WorldItemComponent item, out EntitySlotHandle handle)
  {
    ArgumentNullException.ThrowIfNull(item);
    return TryAllocate(_nextRuntimeEntityId++, item, out handle);
  }

  public bool TryAllocate(
    int runtimeEntityId,
    WorldItemComponent item,
    out EntitySlotHandle handle)
  {
    ArgumentNullException.ThrowIfNull(item);
    handle = default;
    if (runtimeEntityId < 0)
    {
      return false;
    }

    for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
    {
      Slot slot = _slots[slotIndex];
      if (slot.Active || slot.ReuseDelayTicks != 0)
      {
        continue;
      }

      slot.Active = true;
      slot.Item = item;
      slot.RuntimeEntityId = runtimeEntityId;
      handle = EntitySlotHandle.ForPool(
        EntitySlotPoolKind.WorldItem,
        slot.RuntimeEntityId,
        slotIndex,
        slot.Generation);
      return true;
    }

    return false;
  }

  public bool TryGet(EntitySlotHandle handle, out WorldItemComponent? item)
  {
    item = null;
    if (!TryGetSlot(handle, out Slot slot))
    {
      return false;
    }

    item = slot.Item;
    return item is not null;
  }

  public bool TryRelease(EntitySlotHandle handle, int reuseDelayTicks)
  {
    if (reuseDelayTicks < 0 || !TryGetSlot(handle, out Slot slot))
    {
      return false;
    }

    slot.Active = false;
    slot.Item = null;
    slot.ReuseDelayTicks = reuseDelayTicks;
    AdvanceGeneration(slot);
    return true;
  }

  public void AdvanceTick()
  {
    foreach (Slot slot in _slots)
    {
      if (!slot.Active && slot.ReuseDelayTicks > 0)
      {
        slot.ReuseDelayTicks--;
      }
    }
  }

  private bool TryGetSlot(EntitySlotHandle handle, out Slot slot)
  {
    slot = null!;
    if (!handle.MatchesPool(EntitySlotPoolKind.WorldItem) ||
        handle.CompatibilitySlot < 0 ||
        handle.CompatibilitySlot >= _slots.Length)
    {
      return false;
    }

    Slot candidate = _slots[handle.CompatibilitySlot];
    if (!candidate.Active ||
        candidate.RuntimeEntityId != handle.RuntimeEntityId ||
        candidate.Generation != handle.Generation)
    {
      return false;
    }

    slot = candidate;
    return true;
  }

  private static void AdvanceGeneration(Slot slot)
  {
    if (slot.Generation == int.MaxValue)
    {
      throw new InvalidOperationException("The entity slot generation exhausted its range.");
    }

    slot.Generation++;
  }

  private sealed class Slot
  {
    public bool Active { get; set; }

    public int Generation { get; set; } = 1;

    public int RuntimeEntityId { get; set; } = -1;

    public int ReuseDelayTicks { get; set; }

    public WorldItemComponent? Item { get; set; }
  }
}
