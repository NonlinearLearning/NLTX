using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.Share.Attributes;

/// <summary>Tracks periodic damage behaviors owned by active status effect sources.</summary>
public sealed class DamageOverTimeComponent
{
  private readonly Dictionary<long, DamageOverTimeEntry> _entries = [];

  public DamageOverTimeComponent()
  {
  }

  public DamageOverTimeComponent(long sourceId, int periodTicks, int magnitude, int nextTick = 0)
  {
    AddOrReplace(new DamageOverTimeEntry(sourceId, periodTicks, magnitude, nextTick));
  }

  public IReadOnlyList<DamageOverTimeEntry> Entries
    => new ReadOnlyCollection<DamageOverTimeEntry>(_entries.Values.ToArray());

  public void AddOrReplace(DamageOverTimeEntry entry)
  {
    _entries[entry.SourceId] = entry;
  }

  public bool Remove(long sourceId)
  {
    return _entries.Remove(sourceId);
  }

  public bool IsEmpty => _entries.Count == 0;

  public bool TryGet(long sourceId, out DamageOverTimeEntry entry)
  {
    return _entries.TryGetValue(sourceId, out entry);
  }

  public void ScheduleNext(long sourceId, int tick)
  {
    if (_entries.TryGetValue(sourceId, out DamageOverTimeEntry entry))
    {
      _entries[sourceId] = entry with { NextTick = checked(tick + entry.PeriodTicks) };
    }
  }
}

/// <summary>One source-owned periodic damage behavior.</summary>
public readonly record struct DamageOverTimeEntry
{
  public DamageOverTimeEntry(long sourceId, int periodTicks, int magnitude, int nextTick = 0)
  {
    if (sourceId == 0 || periodTicks <= 0 || magnitude <= 0 || nextTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceId));
    }

    SourceId = sourceId;
    PeriodTicks = periodTicks;
    Magnitude = magnitude;
    NextTick = nextTick;
  }

  public long SourceId { get; }

  public int PeriodTicks { get; }

  public int Magnitude { get; }

  public int NextTick { get; init; }

  public bool IsDue(long tick) => tick >= NextTick;
}
