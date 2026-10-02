using System;
using System.Collections.Generic;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerIdAllocator
{
  private readonly HashSet<RevengeMarkerId> _observedIds = new();
  private int _nextValue;

  public RevengeMarkerIdAllocator(int nextValue = 0)
  {
    if (nextValue < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(nextValue));
    }

    _nextValue = nextValue;
  }

  public int NextValue => _nextValue;

  public RevengeMarkerId Allocate()
  {
    if (_nextValue == int.MaxValue)
    {
      throw new InvalidOperationException(
        "The revenge marker ID allocator is exhausted.");
    }

    RevengeMarkerId markerId = new(_nextValue);
    _observedIds.Add(markerId);
    _nextValue++;
    return markerId;
  }

  public void ObserveRestoredId(RevengeMarkerId markerId)
  {
    if (!markerId.IsAssigned)
    {
      throw new ArgumentException(
        "A restored marker ID must be assigned.",
        nameof(markerId));
    }

    if (!_observedIds.Add(markerId))
    {
      throw new InvalidOperationException(
        "The restored revenge marker ID is already in use.");
    }

    if (markerId.Value >= _nextValue)
    {
      _nextValue = markerId.Value == int.MaxValue
        ? int.MaxValue
        : markerId.Value + 1;
    }
  }
}
