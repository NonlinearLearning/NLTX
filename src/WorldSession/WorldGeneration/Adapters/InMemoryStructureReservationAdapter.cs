using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Adapters;

public sealed class InMemoryStructureReservationAdapter :
  IStructureReservationCommitPort
{
  private readonly List<StructureReservationIntent> _reservations = [];

  public ReservationResult Reserve(StructureReservationIntent intent)
  {
    intent.Validate();
    if (!CanPlace(intent))
    {
      return new ReservationResult(
        false,
        intent.ReservationId,
        "The structure overlaps an existing reservation.");
    }

    _reservations.Add(intent);
    return new ReservationResult(true, intent.ReservationId);
  }

  public bool CanPlace(StructureReservationIntent intent)
  {
    intent.Validate();
    foreach (StructureReservationIntent existing in _reservations)
    {
      if (existing.GenerationId == intent.GenerationId &&
          existing.Bounds.Intersects(intent.Bounds))
      {
        return false;
      }
    }

    return true;
  }

  public StructureReservationSnapshot CreateSnapshot()
  {
    return new StructureReservationSnapshot(
      new List<StructureReservationIntent>(_reservations).AsReadOnly());
  }
}
