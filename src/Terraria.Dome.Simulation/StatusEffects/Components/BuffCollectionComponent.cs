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

  public void Add(ushort type, int durationTicks, PlayerHandle source)
  {
    if (type == 0 || durationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    for (int index = 0; index < _entries.Count; index++)
    {
      if (_entries[index].Type != type)
      {
        continue;
      }

      _entries[index] = new BuffEntry(type, durationTicks, source);
      return;
    }

    if (_entries.Count >= MaximumCount)
    {
      throw new InvalidOperationException("The runtime buff capacity was exceeded.");
    }

    _entries.Add(new BuffEntry(type, durationTicks, source));
  }

  public void RemoveAt(int index)
  {
    _entries.RemoveAt(index);
  }

  public void SetAt(int index, BuffEntry entry)
  {
    _entries[index] = entry;
  }
}
