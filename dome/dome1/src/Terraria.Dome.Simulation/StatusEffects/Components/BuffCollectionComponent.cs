using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Components;

public sealed class BuffCollectionComponent
{
  private readonly List<BuffEntry> _entries = new();

  public BuffCollectionComponent(int maximumCount = 44)
  {
    if (maximumCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumCount));
    }

    MaximumCount = maximumCount;
  }

  public IReadOnlyList<BuffEntry> Entries => _entries;

  public int Count => _entries.Count;

  public int MaximumCount { get; }

  public long Revision { get; private set; }

  public bool CanAccept(ushort type)
  {
    if (type == 0)
    {
      return false;
    }

    for (int index = 0; index < _entries.Count; index++)
    {
      if (_entries[index].Type == type)
      {
        return true;
      }
    }

    return _entries.Count < MaximumCount;
  }

  public bool TryAdd(ushort type, int durationTicks, PlayerHandle source)
  {
    if (type == 0 || durationTicks <= 0)
    {
      return false;
    }

    for (int index = 0; index < _entries.Count; index++)
    {
      if (_entries[index].Type != type)
      {
        continue;
      }

      _entries[index] = new BuffEntry(type, durationTicks, source);
      IncrementRevision();
      return true;
    }

    if (_entries.Count >= MaximumCount)
    {
      return false;
    }

    _entries.Add(new BuffEntry(type, durationTicks, source));
    IncrementRevision();
    return true;
  }

  public void Add(ushort type, int durationTicks, PlayerHandle source)
  {
    if (type == 0 || durationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    if (!TryAdd(type, durationTicks, source))
    {
      throw new InvalidOperationException("The runtime buff capacity was exceeded.");
    }
  }

  public void RemoveAt(int index)
  {
    _entries.RemoveAt(index);
    IncrementRevision();
  }

  public void SetAt(int index, BuffEntry entry)
  {
    _entries[index] = entry;
    IncrementRevision();
  }

  private void IncrementRevision()
  {
    if (Revision < long.MaxValue - 1)
    {
      Revision++;
    }
  }
}
