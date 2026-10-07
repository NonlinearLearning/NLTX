namespace Terraria.Player;

public sealed class PlayerStatusEffectSystem
{
  private readonly PlayerBuffSlotsComponent _slots;
  private readonly PlayerBuffImmunityComponent _immunity;
  private readonly PlayerStatusEffectCatalog _catalog;

  public PlayerStatusEffectSystem(
    PlayerBuffSlotsComponent slots,
    PlayerBuffImmunityComponent immunity,
    PlayerStatusEffectCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(slots);
    ArgumentNullException.ThrowIfNull(immunity);
    ArgumentNullException.ThrowIfNull(catalog);

    _slots = slots;
    _immunity = immunity;
    _catalog = catalog;
  }

  public PlayerStatusEffectResult Apply(
    in PlayerStatusEffectApplyCommand command)
  {
    if (command.EffectType.Value <= 0)
    {
      return PlayerStatusEffectResult.Rejected(
        PlayerStatusEffectRejectionReason.InvalidEffectType);
    }

    if (command.DurationTicks <= 0)
    {
      return PlayerStatusEffectResult.Rejected(
        PlayerStatusEffectRejectionReason.NonPositiveDuration);
    }

    if (!_catalog.TryGet(command.EffectType, out PlayerStatusEffectDefinition? definition))
    {
      return PlayerStatusEffectResult.Rejected(
        PlayerStatusEffectRejectionReason.UnknownDefinition);
    }

    if (_immunity.IsImmune(command.EffectType))
    {
      return PlayerStatusEffectResult.Rejected(
        PlayerStatusEffectRejectionReason.Immune);
    }

    CompactExpiredSlots();

    for (int index = 0; index < _slots.Slots.Length; index++)
    {
      BuffSlot current = _slots.Slots[index];
      if (current.IsEmpty || current.Type != command.EffectType)
      {
        continue;
      }

      int remainingTicks = MergeDuration(
        current.RemainingTicks,
        command.DurationTicks,
        definition.AdditiveTimeCap);
      _slots.Slots[index] = new BuffSlot(
        command.EffectType,
        remainingTicks);
      return new PlayerStatusEffectResult(
        Applied: true,
        Refreshed: true,
        Evicted: false,
        SlotIndex: index,
        RemainingTicks: remainingTicks,
        RejectionReason: PlayerStatusEffectRejectionReason.None);
    }

    int freeSlot = FindFreeSlot();
    if (freeSlot < 0)
    {
      int evictionSlot = FindEvictionSlot();
      if (evictionSlot < 0)
      {
        return PlayerStatusEffectResult.Rejected(
          PlayerStatusEffectRejectionReason.NoAvailableSlot);
      }

      RemoveAt(evictionSlot);
      freeSlot = FindFreeSlot();
      if (freeSlot < 0)
      {
        return PlayerStatusEffectResult.Rejected(
          PlayerStatusEffectRejectionReason.NoAvailableSlot);
      }

      _slots.Slots[freeSlot] = new BuffSlot(
        command.EffectType,
        command.DurationTicks);
      return new PlayerStatusEffectResult(
        Applied: true,
        Refreshed: false,
        Evicted: true,
        SlotIndex: freeSlot,
        RemainingTicks: command.DurationTicks,
        RejectionReason: PlayerStatusEffectRejectionReason.None);
    }

    _slots.Slots[freeSlot] = new BuffSlot(
      command.EffectType,
      command.DurationTicks);
    return new PlayerStatusEffectResult(
      Applied: true,
      Refreshed: false,
      Evicted: false,
      SlotIndex: freeSlot,
      RemainingTicks: command.DurationTicks,
      RejectionReason: PlayerStatusEffectRejectionReason.None);
  }

  public bool Remove(ContentId<BuffDefinition> effectType)
  {
    for (int index = 0; index < _slots.Slots.Length; index++)
    {
      if (_slots.Slots[index].IsEmpty || _slots.Slots[index].Type != effectType)
      {
        continue;
      }

      RemoveAt(index);
      return true;
    }

    return false;
  }

  public PlayerStatusEffectTickResult Tick(
    in PlayerStatusEffectTickInput input)
  {
    List<ContentId<BuffDefinition>> expired = [];
    if (input.DecrementTimers)
    {
      for (int index = 0; index < _slots.Slots.Length; index++)
      {
        BuffSlot current = _slots.Slots[index];
        if (current.IsEmpty ||
          !_catalog.TryGet(current.Type, out PlayerStatusEffectDefinition? definition) ||
          !definition.TimerDecreases)
        {
          continue;
        }

        int remainingTicks = current.RemainingTicks - 1;
        _slots.Slots[index] = new BuffSlot(current.Type, remainingTicks);
        if (remainingTicks <= 0)
        {
          expired.Add(current.Type);
        }
      }
    }

    CompactExpiredSlots();
    return new PlayerStatusEffectTickResult(
      TimersAdvanced: input.DecrementTimers,
      ExpiredEffects: expired,
      ActiveCount: CountActiveSlots());
  }

  public void ResetForTick()
  {
    _immunity.ResetForTick();
  }

  public void ResetForLifecycle()
  {
    Array.Clear(_slots.Slots);
    _immunity.ResetForLifecycle();
  }

  public PlayerStatusEffectSnapshot Snapshot()
  {
    return new PlayerStatusEffectSnapshot(_slots.Slots);
  }

  private int FindFreeSlot()
  {
    for (int index = 0; index < _slots.Slots.Length; index++)
    {
      if (_slots.Slots[index].IsEmpty)
      {
        return index;
      }
    }

    return -1;
  }

  private int FindEvictionSlot()
  {
    for (int index = 0; index < _slots.Slots.Length; index++)
    {
      BuffSlot current = _slots.Slots[index];
      if (current.IsEmpty ||
        !_catalog.TryGet(current.Type, out PlayerStatusEffectDefinition? definition))
      {
        continue;
      }

      if (!definition.IsDebuff)
      {
        return index;
      }
    }

    return -1;
  }

  private void CompactExpiredSlots()
  {
    int writeIndex = 0;
    for (int readIndex = 0; readIndex < _slots.Slots.Length; readIndex++)
    {
      BuffSlot current = _slots.Slots[readIndex];
      if (current.IsEmpty)
      {
        continue;
      }

      if (writeIndex != readIndex)
      {
        _slots.Slots[writeIndex] = current;
        _slots.Slots[readIndex] = default;
      }

      writeIndex++;
    }

    while (writeIndex < _slots.Slots.Length)
    {
      _slots.Slots[writeIndex++] = default;
    }
  }

  private void RemoveAt(int index)
  {
    for (int next = index + 1; next < _slots.Slots.Length; next++)
    {
      _slots.Slots[next - 1] = _slots.Slots[next];
    }

    _slots.Slots[^1] = default;
  }

  private static int MergeDuration(
    int current,
    int requested,
    int additiveTimeCap)
  {
    if (additiveTimeCap > 0)
    {
      long sum = (long)current + requested;
      return (int)Math.Min(additiveTimeCap, Math.Min(sum, int.MaxValue));
    }

    return Math.Max(current, requested);
  }

  private int CountActiveSlots()
  {
    int count = 0;
    foreach (BuffSlot slot in _slots.Slots)
    {
      if (!slot.IsEmpty)
      {
        count++;
      }
    }

    return count;
  }
}
