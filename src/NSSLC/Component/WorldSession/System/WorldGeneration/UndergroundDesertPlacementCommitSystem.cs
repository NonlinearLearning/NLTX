using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Gates larva placement commands on a successful structure reservation.
/// </summary>
public static class UndergroundDesertPlacementCommitSystem
{
  public static UndergroundDesertPlacementCommitResult ReserveAndProjectLarvaCommands(
    IStructureReservationCommitPort reservationPort,
    in StructureReservationIntent reservation,
    in UndergroundDesertLarvaPlacementSnapshot larvaSnapshot)
  {
    ArgumentNullException.ThrowIfNull(reservationPort);
    reservation.Validate();
    if (reservation.GenerationId != larvaSnapshot.GenerationId)
    {
      throw new ArgumentException(
        "Underground-desert placement inputs must belong to the same generation.",
        nameof(larvaSnapshot));
    }

    // Build the pure command projection before the first external effect so invalid
    // snapshot data cannot leave a reservation behind.
    UndergroundDesertLarvaPlacementCommand[] projectedCommands =
      UndergroundDesertLarvaPlacementProjection.CreateCommands(in larvaSnapshot);

    ReservationResult reservationResult = reservationPort.Reserve(reservation);
    if (!reservationResult.Accepted)
    {
      return new UndergroundDesertPlacementCommitResult(
        reservationResult,
        Array.Empty<UndergroundDesertLarvaPlacementCommand>());
    }

    IReadOnlyList<UndergroundDesertLarvaPlacementCommand> commands =
      Array.AsReadOnly(projectedCommands);
    return new UndergroundDesertPlacementCommitResult(
      reservationResult,
      commands);
  }
}
