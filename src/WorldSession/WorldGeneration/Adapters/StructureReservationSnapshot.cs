using System.Collections.Generic;

namespace Terraria.WorldGeneration.Adapters;

public sealed class StructureReservationSnapshot
{
  public StructureReservationSnapshot(
    IReadOnlyList<StructureReservationIntent> reservations)
  {
    Reservations = reservations;
  }

  public IReadOnlyList<StructureReservationIntent> Reservations { get; }

  public int Count => Reservations.Count;
}
