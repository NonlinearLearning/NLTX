using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileRopeEndFramingQuery
{
  public static IReadOnlyList<TileFrameRequest> CreateRequests(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingMutations,
    int rangeToCheck = 5)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(pendingMutations);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (!TileRopeQuery.IsRope(snapshot, tileDefinitions, x, y, rangeToCheck))
    {
      return Array.Empty<TileFrameRequest>();
    }

    TileRopeEnds ends = TileRopeQuery.FindEnds(snapshot, x, y, rangeToCheck: rangeToCheck);
    List<TileFrameRequest> requests = new(2);
    AddRequest(ends.TopY);
    AddRequest(ends.BottomY);
    return requests;

    void AddRequest(int endY)
    {
      if (endY < 0 || requests.Exists(request => request.X == x && request.Y == endY))
      {
        return;
      }

      requests.Add(new TileFrameRequest(
        x,
        endY,
        TileFrameMutationKind.RopeEnd,
        snapshot,
        pendingMutations));
    }
  }
}
