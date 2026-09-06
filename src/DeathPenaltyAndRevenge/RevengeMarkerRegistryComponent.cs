using System;
using System.Collections.Generic;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerRegistryComponent
{
  private readonly HashSet<RevengeMarkerId> _markerIds;

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

      _markerIds.Add(markerId);
    }
  }

  public IReadOnlySet<RevengeMarkerId> MarkerIds => _markerIds;

  internal bool TryRegister(RevengeMarkerId markerId)
  {
    if (!markerId.IsAssigned)
    {
      return false;
    }

    return _markerIds.Add(markerId);
  }

  internal bool TryUnregister(RevengeMarkerId markerId)
  {
    return _markerIds.Remove(markerId);
  }

  internal void Clear()
  {
    _markerIds.Clear();
  }
}
