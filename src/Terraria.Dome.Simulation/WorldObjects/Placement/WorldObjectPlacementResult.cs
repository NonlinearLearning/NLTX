using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public enum WorldObjectPlacementStatus
{
  Committed = 0,
  Rejected = 1,
  Deferred = 2
}

public sealed record WorldObjectPlacementResult(
  WorldObjectPlacementStatus Status,
  WorldObjectPlacementFailureCode FailureCode,
  long Sequence,
  IReadOnlyList<WorldSectionCoordinates> Sections)
{
  public bool Committed => Status == WorldObjectPlacementStatus.Committed;

  public static WorldObjectPlacementResult Rejected(
    long sequence,
    WorldObjectPlacementFailureCode failureCode)
  {
    return new(
      WorldObjectPlacementStatus.Rejected,
      failureCode,
      sequence,
      System.Array.Empty<WorldSectionCoordinates>());
  }
}
