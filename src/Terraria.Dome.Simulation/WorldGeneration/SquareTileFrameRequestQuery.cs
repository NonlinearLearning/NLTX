using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SquareTileFrameRequestQuery
{
  public static IReadOnlyList<TileFrameRequest> CreateRequests(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    TileFrameMutationKind mutationKind,
    IReadOnlyCollection<TileChangeCommand> pendingMutations)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingMutations);
    List<TileFrameRequest> requests = new(9);
    for (int offsetY = -1; offsetY <= 1; offsetY++)
    {
      for (int offsetX = -1; offsetX <= 1; offsetX++)
      {
        int requestX = x + offsetX;
        int requestY = y + offsetY;
        if (snapshot.Metadata.IsInside(requestX, requestY))
        {
          requests.Add(new TileFrameRequest(
            requestX,
            requestY,
            mutationKind,
            snapshot,
            pendingMutations));
        }
      }
    }

    return requests;
  }
}
