namespace Terraria.EntityLifecycleAttribution;

public sealed class NpcEntitySlotStore
{
  private readonly Slot[] _slots;
  private int _nextRuntimeEntityId;

  public NpcEntitySlotStore(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = Enumerable.Range(0, capacity).Select(_ => new Slot()).ToArray();
  }

  public int Capacity => _slots.Length;

  public bool TryAllocate(int runtimeEntityId, out EntitySlotHandle handle)
  {
    handle = default;
    if (runtimeEntityId < 0)
    {
      return false;
    }

    for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
    {
      Slot slot = _slots[slotIndex];
      if (slot.Active)
      {
        continue;
      }

      slot.Active = true;
      slot.RuntimeEntityId = runtimeEntityId;
      handle = EntitySlotHandle.ForPool(
        EntitySlotPoolKind.Npc,
        runtimeEntityId,
        slotIndex,
        slot.Generation);
      return true;
    }

    return false;
  }

  public bool TryAllocate(out EntitySlotHandle handle)
  {
    return TryAllocate(_nextRuntimeEntityId++, out handle);
  }

  public bool TryRelease(EntitySlotHandle handle)
  {
    if (!handle.MatchesPool(EntitySlotPoolKind.Npc) ||
        handle.CompatibilitySlot < 0 ||
        handle.CompatibilitySlot >= _slots.Length)
    {
      return false;
    }

    Slot slot = _slots[handle.CompatibilitySlot];
    if (!slot.Active || slot.RuntimeEntityId != handle.RuntimeEntityId || slot.Generation != handle.Generation)
    {
      return false;
    }

    slot.Active = false;
    slot.RuntimeEntityId = -1;
    AdvanceGeneration(slot);
    return true;
  }

  private static void AdvanceGeneration(Slot slot)
  {
    if (slot.Generation == int.MaxValue)
    {
      throw new InvalidOperationException("The NPC slot generation exhausted its range.");
    }

    slot.Generation++;
  }

  private sealed class Slot
  {
    public bool Active { get; set; }

    public int Generation { get; set; } = 1;

    public int RuntimeEntityId { get; set; } = -1;
  }
}
