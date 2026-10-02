using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Structures;

public sealed class WorldStructureReservationSnapshot
{
  public WorldStructureReservationSnapshot(
    long generationId,
    IReadOnlyList<WorldStructureReservationRecord> reservations)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(reservations);
    GenerationId = generationId;
    Reservations = new List<WorldStructureReservationRecord>(reservations)
      .AsReadOnly();
  }

  public long GenerationId { get; }

  public IReadOnlyList<WorldStructureReservationRecord> Reservations { get; }

  public int Count => Reservations.Count;
}
