namespace Terraria.EntityLifecycleAttribution;

public sealed class PresentationEffectPoolStore
{
  private readonly Dictionary<PresentationEffectKind, Slot[]> _slots;
  private int _nextRuntimeEntityId;

  public PresentationEffectPoolStore(int capacityPerKind)
  {
    if (capacityPerKind <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacityPerKind));
    }

    _slots = Enum.GetValues<PresentationEffectKind>()
      .ToDictionary(kind => kind, _ => Enumerable.Range(0, capacityPerKind).Select(_ => new Slot()).ToArray());
  }

  public bool TryAllocate(
    PresentationEffectKind kind,
    int lifetimeTicks,
    out EntitySlotHandle handle)
  {
    return TryAllocate(kind, _nextRuntimeEntityId++, lifetimeTicks, out handle);
  }

  public bool TryAllocate(
    PresentationEffectKind kind,
    int runtimeEntityId,
    int lifetimeTicks,
    out EntitySlotHandle handle)
  {
    handle = default;
    if (runtimeEntityId < 0 || lifetimeTicks <= 0 || !_slots.TryGetValue(kind, out Slot[]? slots))
    {
      return false;
    }

    for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
    {
      Slot slot = slots[slotIndex];
      if (slot.Active)
      {
        continue;
      }

      slot.Active = true;
      slot.RemainingTicks = lifetimeTicks;
      slot.RuntimeEntityId = runtimeEntityId;
      handle = EntitySlotHandle.ForPresentation(
        kind,
        slot.RuntimeEntityId,
        slotIndex,
        slot.Generation);
      return true;
    }

    return false;
  }

  public void AdvanceTick()
  {
    foreach (Slot[] slots in _slots.Values)
    {
      foreach (Slot slot in slots)
      {
        if (!slot.Active)
        {
          continue;
        }

        slot.RemainingTicks--;
        if (slot.RemainingTicks == 0)
        {
          slot.Active = false;
          slot.RuntimeEntityId = -1;
          AdvanceGeneration(slot);
        }
      }
    }
  }

  public bool IsActive(PresentationEffectKind kind, EntitySlotHandle handle)
  {
    if (!handle.MatchesPool(EntitySlotPoolKind.PresentationEffect, kind) ||
        !_slots.TryGetValue(kind, out Slot[]? slots) ||
        handle.CompatibilitySlot < 0 ||
        handle.CompatibilitySlot >= slots.Length)
    {
      return false;
    }

    Slot slot = slots[handle.CompatibilitySlot];
    return slot.Active && slot.RuntimeEntityId == handle.RuntimeEntityId && slot.Generation == handle.Generation;
  }

  public bool TryRelease(PresentationEffectKind kind, EntitySlotHandle handle)
  {
    if (!handle.MatchesPool(EntitySlotPoolKind.PresentationEffect, kind) ||
        !_slots.TryGetValue(kind, out Slot[]? slots) ||
        handle.CompatibilitySlot < 0 ||
        handle.CompatibilitySlot >= slots.Length)
    {
      return false;
    }

    Slot slot = slots[handle.CompatibilitySlot];
    if (!slot.Active || slot.RuntimeEntityId != handle.RuntimeEntityId || slot.Generation != handle.Generation)
    {
      return false;
    }

    slot.Active = false;
    slot.RuntimeEntityId = -1;
    slot.RemainingTicks = 0;
    AdvanceGeneration(slot);
    return true;
  }

  private static void AdvanceGeneration(Slot slot)
  {
    if (slot.Generation == int.MaxValue)
    {
      throw new InvalidOperationException("The presentation slot generation exhausted its range.");
    }

    slot.Generation++;
  }

  private sealed class Slot
  {
    public bool Active { get; set; }

    public int Generation { get; set; } = 1;

    public int RuntimeEntityId { get; set; } = -1;

    public int RemainingTicks { get; set; }
  }
}
