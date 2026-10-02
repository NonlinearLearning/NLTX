using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Actions;

/// <summary>
/// Reports the reservation result and the effects that may be published after it.
/// </summary>
public readonly record struct UndergroundDesertPlacementCommitResult(
  ReservationResult Reservation,
  IReadOnlyList<UndergroundDesertLarvaPlacementCommand> LarvaCommands)
{
  public bool ReservationAccepted => Reservation.Accepted;

  public int LarvaCommandCount => LarvaCommands.Count;
}
