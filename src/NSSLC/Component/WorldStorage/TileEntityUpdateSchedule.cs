using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.WorldStorage;

public sealed class TileEntityUpdateSchedule : IDisposable
{
  private HashSet<TileEntityId> _scheduledIds = new();
  private TileEntityId[] _tickSnapshot = Array.Empty<TileEntityId>();
  private long _scheduleRevision;
  private bool _isDisposed;

  public int Count
  {
    get
    {
      VerifyAccess();
      return _scheduledIds.Count;
    }
  }

  public long ScheduleRevision
  {
    get
    {
      VerifyAccess();
      return _scheduleRevision;
    }
  }

  public bool IsScheduled(TileEntityId id)
  {
    VerifyAccess();
    return _scheduledIds.Contains(id);
  }

  public bool Schedule(TileEntityId id)
  {
    VerifyAccess();
    if (id.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }
    if (!_scheduledIds.Add(id))
    {
      return false;
    }
    _scheduleRevision++;
    return true;
  }

  public bool Unschedule(TileEntityId id)
  {
    VerifyAccess();
    if (!_scheduledIds.Remove(id))
    {
      return false;
    }
    _scheduleRevision++;
    return true;
  }

  public void Replace(IEnumerable<TileEntityId> ids)
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(ids);
    var replacement = new HashSet<TileEntityId>();
    foreach (TileEntityId id in ids)
    {
      if (id.Value < 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(ids),
          "Scheduled tile entity IDs must be non-negative.");
      }
      replacement.Add(id);
    }
    if (_scheduledIds.SetEquals(replacement))
    {
      return;
    }
    _scheduledIds = replacement;
    _tickSnapshot = Array.Empty<TileEntityId>();
    _scheduleRevision++;
  }

  public IReadOnlyList<TileEntityId> CaptureTickSnapshot()
  {
    VerifyAccess();
    _tickSnapshot = _scheduledIds.OrderBy(static id => id.Value).ToArray();
    return Array.AsReadOnly((TileEntityId[])_tickSnapshot.Clone());
  }

  public void CompleteTickSnapshot()
  {
    VerifyAccess();
    _tickSnapshot = Array.Empty<TileEntityId>();
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _scheduledIds.Clear();
    _tickSnapshot = Array.Empty<TileEntityId>();
    _isDisposed = true;
  }

  private void VerifyAccess()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }
}
