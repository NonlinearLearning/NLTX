using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameEvaluationQuery
{
  private const int FrameWidth = 18;

  public static TileFrameEvaluationResult Evaluate(TileFrameRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    WorldGridSnapshot snapshot = request.Snapshot;
    if (!snapshot.Metadata.IsInside(request.X, request.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(request));
    }

    WorldTile tile = GetPendingTile(snapshot, request.X, request.Y, request.PendingMutations);
    IReadOnlyList<TileFrameCoordinate> affectedCoordinates = GetAffectedCoordinates(
      snapshot,
      request.X,
      request.Y);
    TileFrameClassificationKind classification = TileFrameClassificationQuery.Classify(tile);
    if (request.MutationKind == TileFrameMutationKind.RopeEnd ||
        classification != TileFrameClassificationKind.OrdinarySolid)
    {
      return new TileFrameEvaluationResult(
        false,
        tile.FrameX,
        tile.FrameY,
        null,
        null,
        false,
        affectedCoordinates,
        classification);
    }

    int activeNeighborCount = 0;
    foreach (TileFrameCoordinate neighbor in GetCardinalCoordinates(request.X, request.Y))
    {
      if (!snapshot.Metadata.IsInside(neighbor.X, neighbor.Y))
      {
        continue;
      }

      WorldTile neighborTile = GetPendingTile(
        snapshot,
        neighbor.X,
        neighbor.Y,
        request.PendingMutations);
      if (neighborTile.IsActive)
      {
        activeNeighborCount++;
      }
    }

    return new TileFrameEvaluationResult(
      true,
      checked((short)(activeNeighborCount * FrameWidth)),
      0,
      false,
      0,
      false,
      affectedCoordinates,
      classification);
  }

  public static IReadOnlyList<TileFrameRequest> CreateRequests(
    WorldGridSnapshot snapshot,
    IReadOnlyCollection<TileChangeCommand> pendingMutations,
    TileFrameMutationKind mutationKind)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingMutations);
    SortedSet<TileFrameCoordinate> coordinates = new();
    foreach (TileChangeCommand mutation in pendingMutations)
    {
      foreach (TileFrameCoordinate coordinate in GetAffectedCoordinates(
        snapshot,
        mutation.X,
        mutation.Y))
      {
        _ = coordinates.Add(coordinate);
      }
    }

    List<TileFrameRequest> requests = new(coordinates.Count);
    foreach (TileFrameCoordinate coordinate in coordinates)
    {
      requests.Add(new TileFrameRequest(
        coordinate.X,
        coordinate.Y,
        mutationKind,
        snapshot,
        pendingMutations));
    }

    return requests;
  }

  private static IReadOnlyList<TileFrameCoordinate> GetAffectedCoordinates(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    List<TileFrameCoordinate> coordinates = new(5);
    foreach (TileFrameCoordinate coordinate in new[]
    {
      new TileFrameCoordinate(x, y),
      new TileFrameCoordinate(x - 1, y),
      new TileFrameCoordinate(x + 1, y),
      new TileFrameCoordinate(x, y - 1),
      new TileFrameCoordinate(x, y + 1)
    })
    {
      if (snapshot.Metadata.IsInside(coordinate.X, coordinate.Y))
      {
        coordinates.Add(coordinate);
      }
    }

    return coordinates;
  }

  private static IEnumerable<TileFrameCoordinate> GetCardinalCoordinates(int x, int y)
  {
    yield return new TileFrameCoordinate(x - 1, y);
    yield return new TileFrameCoordinate(x + 1, y);
    yield return new TileFrameCoordinate(x, y - 1);
    yield return new TileFrameCoordinate(x, y + 1);
  }

  private static WorldTile GetPendingTile(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingMutations)
  {
    return TileMutationProjection.ApplyPending(
      snapshot.GetTile(x, y),
      x,
      y,
      pendingMutations);
  }

}
