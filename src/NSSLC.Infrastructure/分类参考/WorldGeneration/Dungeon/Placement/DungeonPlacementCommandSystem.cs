namespace Terraria.WorldGeneration.Dungeon.Placement;

public sealed class DungeonPlacementCommandSystem
{
  public DungeonPlacementDecision ValidatePlatform(
    DungeonPlatformPlacementRequest request,
    DungeonPlacementSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(request);
    DungeonPlacementDecision decision = Queries.DungeonPlacementQuery.Evaluate(
      snapshot,
      request.ForcePlacement);
    if (!decision.Accepted)
    {
      return decision;
    }

    return request.CanPlaceHereCallback?.Invoke(snapshot) == false
      ? DungeonPlacementDecision.Reject("Placement callback rejected the candidate.")
      : DungeonPlacementDecision.Accept();
  }

  public DungeonPlacementDecision ValidateDoor(
    DungeonDoorPlacementRequest request,
    DungeonPlacementSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(request);
    return Queries.DungeonPlacementQuery.Evaluate(snapshot, request.AlwaysClearArea);
  }
}
