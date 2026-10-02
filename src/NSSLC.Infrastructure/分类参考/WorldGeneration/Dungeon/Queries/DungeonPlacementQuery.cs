using Terraria.WorldGeneration.Dungeon.Placement;

namespace Terraria.WorldGeneration.Dungeon.Queries;

public static class DungeonPlacementQuery
{
  public static DungeonPlacementDecision Evaluate(
    DungeonPlacementSnapshot snapshot,
    bool forcePlacement = false)
  {
    if (snapshot.OccupancyRevision < 0)
    {
      return DungeonPlacementDecision.Reject("Placement snapshot revision is invalid.");
    }

    if (snapshot.InProtectedBounds && !forcePlacement)
    {
      return DungeonPlacementDecision.Reject("Placement intersects protected bounds.");
    }

    if (snapshot.Occupied && !forcePlacement)
    {
      return DungeonPlacementDecision.Reject("Placement target is occupied.");
    }

    return DungeonPlacementDecision.Accept();
  }
}
