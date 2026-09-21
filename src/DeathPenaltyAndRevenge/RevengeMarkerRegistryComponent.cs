using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerRegistryComponent
{
  private readonly HashSet<RevengeMarkerId> _markerIds;
  private FrozenSet<RevengeMarkerId> _markerIdsView;

  public RevengeMarkerRegistryComponent()
    : this(Array.Empty<RevengeMarkerId>())
  {
  }

  public RevengeMarkerRegistryComponent(IEnumerable<RevengeMarkerId> markerIds)
  {
    ArgumentNullException.ThrowIfNull(markerIds);

    _markerIds = new HashSet<RevengeMarkerId>();
    foreach (RevengeMarkerId markerId in markerIds)
    {
      if (!markerId.IsAssigned)
      {
        throw new ArgumentException(
          "A marker registry can contain only assigned marker IDs.",
          nameof(markerIds));
      }

      if (!_markerIds.Add(markerId))
      {
        throw new ArgumentException(
          "A marker registry cannot contain duplicate marker IDs.",
          nameof(markerIds));
      }
    }

    _markerIdsView = _markerIds.ToFrozenSet();
  }

  public IReadOnlySet<RevengeMarkerId> MarkerIds => _markerIdsView;

  internal bool TryRegister(RevengeMarkerId markerId)
  {
    if (!markerId.IsAssigned)
    {
      return false;
    }

    if (!_markerIds.Add(markerId))
    {
      return false;
    }

    RefreshMarkerIdView();
    return true;
  }

  internal bool TryUnregister(RevengeMarkerId markerId)
  {
    if (!_markerIds.Remove(markerId))
    {
      return false;
    }

    RefreshMarkerIdView();
    return true;
  }

  internal void Clear()
  {
    _markerIds.Clear();
    RefreshMarkerIdView();
  }

  private void RefreshMarkerIdView()
  {
    _markerIdsView = _markerIds.ToFrozenSet();
  }
}
