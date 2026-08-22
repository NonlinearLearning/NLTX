using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileMergeFrametestResult(
  TileMergeNeighbors Neighbors,
  bool FrameUp,
  bool FrameDown,
  bool FrameLeft,
  bool FrameRight)
{
  public IReadOnlyList<TileFrameRequest> CreateFrameRequests(
    int x,
    int y,
    WorldGridSnapshot snapshot,
    IReadOnlyCollection<TileChangeCommand> pendingMutations)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingMutations);
    List<TileFrameRequest> requests = new(4);
    AddIfRequested(x, y - 1, FrameUp);
    AddIfRequested(x, y + 1, FrameDown);
    AddIfRequested(x - 1, y, FrameLeft);
    AddIfRequested(x + 1, y, FrameRight);
    return requests;

    void AddIfRequested(int requestX, int requestY, bool shouldFrame)
    {
      if (shouldFrame && snapshot.Metadata.IsInside(requestX, requestY))
      {
        requests.Add(new TileFrameRequest(
          requestX,
          requestY,
          TileFrameMutationKind.TileMergeFrametest,
          snapshot,
          pendingMutations));
      }
    }
  }
}
