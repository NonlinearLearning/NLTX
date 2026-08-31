using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidWallFrameRequestQuery
{
  public const int RequestsPerCenter = 9;
  public const int SourceLine = 28425;
  public const string Source = "worldgen.Pyramid.wall-frame";

  public static bool TryCreateRequests(
    WorldGridSnapshot snapshot,
    IReadOnlyList<WallFrameCoordinate> centers,
    out IReadOnlyList<LegacyPyramidWallFrameRequest> requests,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(centers);
    requests = Array.Empty<LegacyPyramidWallFrameRequest>();
    failureReason = null;
    if (!TryGetRequestCapacity(centers.Count, out int requestCapacity))
    {
      failureReason = "Pyramid wall-framing request capacity overflowed.";
      return false;
    }

    for (int centerIndex = 0; centerIndex < centers.Count; centerIndex++)
    {
      WallFrameCoordinate center = centers[centerIndex];
      if (!snapshot.Metadata.IsInside(center.X, center.Y))
      {
        failureReason = "Pyramid wall-framing center was outside the world.";
        return false;
      }

      if (!HasInsideNeighborhood(snapshot.Metadata, center))
      {
        failureReason = "Pyramid wall-framing request crossed the world edge.";
        return false;
      }
    }

    List<LegacyPyramidWallFrameRequest> expanded = new(requestCapacity);
    for (int centerIndex = 0; centerIndex < centers.Count; centerIndex++)
    {
      WallFrameCoordinate center = centers[centerIndex];
      for (int offsetX = -1; offsetX <= 1; offsetX++)
      {
        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
          expanded.Add(new LegacyPyramidWallFrameRequest(
            center,
            new WallFrameCoordinate(center.X + offsetX, center.Y + offsetY),
            ResetFrame: true,
            Source,
            SourceLine));
        }
      }
    }

    requests = expanded.AsReadOnly();
    return true;
  }

  private static bool HasInsideNeighborhood(
    WorldMetadata metadata,
    WallFrameCoordinate center)
  {
    return center.X > 1 &&
      center.X < metadata.Width - 2 &&
      center.Y > 1 &&
      center.Y < metadata.Height - 2;
  }

  private static bool TryGetRequestCapacity(int centerCount, out int capacity)
  {
    try
    {
      capacity = checked(centerCount * RequestsPerCenter);
      return true;
    }
    catch (OverflowException)
    {
      capacity = 0;
      return false;
    }
  }
}
