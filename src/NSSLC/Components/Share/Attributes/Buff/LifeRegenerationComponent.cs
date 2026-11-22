using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.Attributes;

/// <summary>Tracks periodic healing behaviors owned by active status effect sources.</summary>
public sealed class LifeRegenerationComponent
{
  private readonly Dictionary<long, LifeRegenerationEntry> _entries = [];

  public LifeRegenerationComponent()
  {
  }

  public LifeRegenerationComponent(long sourceId, int periodTicks, int magnitude, int nextTick = 0)
  {
    AddOrReplace(new LifeRegenerationEntry(sourceId, periodTicks, magnitude, nextTick));
  }

  public IReadOnlyList<LifeRegenerationEntry> Entries
    => new ReadOnlyCollection<LifeRegenerationEntry>(_entries.Values.ToArray());

  public void AddOrReplace(LifeRegenerationEntry entry)
  {
    _entries[entry.SourceId] = entry;
  }

  public bool Remove(long sourceId)
  {
    return _entries.Remove(sourceId);
  }

  public bool IsEmpty => _entries.Count == 0;

  public bool TryGet(long sourceId, out LifeRegenerationEntry entry)
  {
    return _entries.TryGetValue(sourceId, out entry);
  }

  public void ScheduleNext(long sourceId, int tick)
  {
    if (_entries.TryGetValue(sourceId, out LifeRegenerationEntry entry))
    {
      _entries[sourceId] = entry with { NextTick = checked(tick + entry.PeriodTicks) };
    }
  }
}

/// <summary>One source-owned periodic healing behavior.</summary>
public readonly record struct LifeRegenerationEntry
{
  public LifeRegenerationEntry(long sourceId, int periodTicks, int magnitude, int nextTick = 0)
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
