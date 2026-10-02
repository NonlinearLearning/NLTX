namespace Terraria.WorldStorage;

public sealed class EntitySlotStore<TState, TSlot>
  where TState : class
  where TSlot : struct
{
  private EntitySlotEntry<TState>[] _entries = Array.Empty<EntitySlotEntry<TState>>();
  private readonly Func<TSlot, int> _getSlotValue;
  private readonly Func<int, TSlot> _createSlot;
  private readonly int _maximumCapacity;
  private int _activeCount;

  public int Capacity => _entries.Length;
  public int ActiveCount => _activeCount;

  public EntitySlotStore(
    Func<TSlot, int> getSlotValue,
    Func<int, TSlot> createSlot,
    int maximumCapacity = int.MaxValue)
  {
    ArgumentNullException.ThrowIfNull(getSlotValue);
    ArgumentNullException.ThrowIfNull(createSlot);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumCapacity);

    _getSlotValue = getSlotValue;
    _createSlot = createSlot;
    _maximumCapacity = maximumCapacity;
  }

  public bool TryAllocate(TState state, out TSlot slot, out uint generation)
  {
    ArgumentNullException.ThrowIfNull(state);

    for (int i = 0; i < _entries.Length; i++)
    {
      EntitySlotEntry<TState> entry = _entries[i];
      if (entry.IsOccupied || entry.IsGenerationExhausted)
      {
        continue;
      }

      if (entry.Generation == uint.MaxValue)
      {
        entry.IsGenerationExhausted = true;
        continue;
      }

      slot = CreateSlot(i);
      entry.Generation++;
      entry.State = state;
      entry.IsOccupied = true;
      _activeCount++;
      generation = entry.Generation;
      return true;
    }

    if (_entries.Length >= _maximumCapacity)
    {
      slot = default;
      generation = 0;
      return false;
    }

    int newCapacity = GetNextCapacity();
    int oldCapacity = _entries.Length;
    Array.Resize(ref _entries, newCapacity);
    for (int i = oldCapacity; i < newCapacity; i++)
    {
      _entries[i] = new EntitySlotEntry<TState>();
    }

    slot = CreateSlot(oldCapacity);
    EntitySlotEntry<TState> newEntry = _entries[oldCapacity];
    newEntry.Generation = 1;
    newEntry.State = state;
    newEntry.IsOccupied = true;
    _activeCount++;
    generation = newEntry.Generation;
    return true;
  }

  public bool TryAllocateAt(TSlot slot, TState state, out uint generation)
  {
    ArgumentNullException.ThrowIfNull(state);

    int index = GetSlotValue(slot);
    if (index >= _maximumCapacity)
    {
      generation = 0;
      return false;
    }

    _ = CreateSlot(index);
    if (!TryEnsureCapacity(index + 1))
    {
      generation = 0;
      return false;
    }

    EntitySlotEntry<TState> entry = _entries[index];
    if (entry.IsOccupied || entry.IsGenerationExhausted)
    {
      generation = 0;
      return false;
    }

    if (entry.Generation == uint.MaxValue)
    {
      entry.IsGenerationExhausted = true;
      generation = 0;
      return false;
    }

    entry.Generation++;
    entry.State = state;
    entry.IsOccupied = true;
    _activeCount++;
    generation = entry.Generation;
    return true;
  }

  public bool TryGet(TSlot slot, uint generation, out TState? state)
  {
    int index = GetSlotValue(slot);
    if (!TryGetEntry(index, generation, out EntitySlotEntry<TState> entry))
    {
      state = null;
      return false;
    }

    state = entry.State;
    return true;
  }

  public bool TryGetOccupiedAt(
    int index,
    out TSlot slot,
    out uint generation,
    out TState? state)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    if (index >= _entries.Length || !_entries[index].IsOccupied)
    {
      slot = default;
      generation = 0;
      state = null;
      return false;
    }

    EntitySlotEntry<TState> entry = _entries[index];
    state = entry.State ?? throw new InvalidOperationException(
      "An occupied entity slot has no state.");
    slot = CreateSlot(index);
    generation = entry.Generation;
    return true;
  }

  public bool TryReplace(
    TSlot slot,
    uint expectedGeneration,
    TState replacement,
    out uint replacementGeneration)
  {
    ArgumentNullException.ThrowIfNull(replacement);

    int index = GetSlotValue(slot);
    if (!TryGetEntry(index, expectedGeneration, out EntitySlotEntry<TState> entry) ||
      entry.Generation == uint.MaxValue)
    {
      replacementGeneration = 0;
      return false;
    }

    entry.Generation++;
    entry.State = replacement;
    replacementGeneration = entry.Generation;
    return true;
  }

  public bool TryRelease(TSlot slot, uint expectedGeneration, out TState? releasedState)
  {
    int index = GetSlotValue(slot);
    if (!TryGetEntry(index, expectedGeneration, out EntitySlotEntry<TState> entry))
    {
      releasedState = null;
      return false;
    }

    releasedState = entry.State;
    entry.State = null;
    entry.IsOccupied = false;
    entry.IsGenerationExhausted = entry.Generation == uint.MaxValue;
    _activeCount--;
    return true;
  }

  private int GetNextCapacity()
  {
    if (_entries.Length == 0)
    {
      return 1;
    }

    long doubledCapacity = (long)_entries.Length * 2;
    return (int)Math.Min(doubledCapacity, _maximumCapacity);
  }

  private bool TryEnsureCapacity(int requiredCapacity)
  {
    if (requiredCapacity <= _entries.Length)
    {
      return true;
    }

    if (requiredCapacity > _maximumCapacity)
    {
      return false;
    }

    int newCapacity = Math.Max(requiredCapacity, GetNextCapacity());
    if (newCapacity > _maximumCapacity)
    {
      newCapacity = _maximumCapacity;
    }

    if (newCapacity < requiredCapacity)
    {
      return false;
    }

    int oldCapacity = _entries.Length;
    Array.Resize(ref _entries, newCapacity);
    for (int i = oldCapacity; i < newCapacity; i++)
    {
      _entries[i] = new EntitySlotEntry<TState>();
    }

    return true;
  }

  private int GetSlotValue(TSlot slot)
  {
    int value = _getSlotValue(slot);
    ArgumentOutOfRangeException.ThrowIfNegative(value);
    return value;
  }

  private TSlot CreateSlot(int value)
  {
    TSlot slot = _createSlot(value);
    if (GetSlotValue(slot) != value)
    {
      throw new InvalidOperationException(
        "The slot factory did not preserve the requested slot value.");
    }

    return slot;
  }

  private bool TryGetEntry(
    int index,
    uint generation,
    out EntitySlotEntry<TState> entry)
  {
    if (generation == 0 || index >= _entries.Length)
    {
      entry = null!;
      return false;
    }

    EntitySlotEntry<TState> candidate = _entries[index];
    if (!candidate.IsOccupied || candidate.Generation != generation)
    {
      entry = null!;
      return false;
    }

    entry = candidate;
    return true;
  }
}
