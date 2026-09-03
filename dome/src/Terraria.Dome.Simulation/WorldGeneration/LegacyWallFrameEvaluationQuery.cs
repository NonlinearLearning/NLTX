using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWallFrameEvaluationQuery
{
  private const ushort GlassWallType = 21;

  public static LegacyWallFrameEvaluationResult Evaluate(
    WorldGridSnapshot snapshot,
    WallFrameCoordinate target,
    bool resetFrame,
    bool showInvisibleWalls,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    if (!snapshot.Metadata.IsInside(target.X, target.Y))
    {
      return CreateSkippedResult(target, "Target was outside the world.");
    }

    if (target.X <= 0 || target.Y <= 0 ||
        target.X >= snapshot.Metadata.Width - 1 ||
        target.Y >= snapshot.Metadata.Height - 1)
    {
      return CreateSkippedResult(target, "Target was on the world edge.");
    }

    WorldTile current = snapshot.GetTile(target.X, target.Y);
    ushort wallType = current.WallType >= LegacyLargeFrameWallRegistry.WallTypeCount
      ? (ushort)0
      : current.WallType;
    if (wallType == 0)
    {
      WorldTile cleared = current with
      {
        WallType = 0,
        WallColor = 0,
        IsInvisibleWall = false,
        IsFullbrightWall = false
      };
      return new LegacyWallFrameEvaluationResult(
        true,
        target,
        cleared,
        0,
        -1,
        0,
        true,
        null);
    }

    if (!LegacyWallFrameNeighborQuery.TryEvaluate(
          snapshot,
          target.X,
          target.Y,
          showInvisibleWalls,
          out LegacyWallFrameNeighborResult neighborResult,
          out string? neighborFailure))
    {
      throw new InvalidOperationException(
        neighborFailure ?? "Wall-frame neighbor classification failed unexpectedly.");
    }

    int neighborMask = (int)neighborResult.NeighborMask;
    int frameNumber;
    int randomDrawCount = 0;
    byte frameSize = LegacyLargeFrameWallRegistry.GetFrameSize(wallType);
    if (frameSize == 1)
    {
      frameNumber = LegacyWallFrameLookupRegistry.GetPhlebasFrameNumber(
        target.Y % 4,
        target.X % 3);
    }
    else if (frameSize == 2)
    {
      frameNumber = LegacyWallFrameLookupRegistry.GetLazureFrameNumber(
        target.X % 2,
        target.Y % 2);
    }
    else if (resetFrame)
    {
      frameNumber = random.Next(0, 3);
      randomDrawCount++;
      if (wallType == GlassWallType && random.Next(2) == 0)
      {
        frameNumber = 2;
      }

      if (wallType == GlassWallType)
      {
        randomDrawCount++;
      }
    }
    else
    {
      frameNumber = current.WallFrameNumber & 3;
    }

    int frameLookupIndex = neighborMask;
    if (neighborMask == 15)
    {
      frameLookupIndex += LegacyWallFrameLookupRegistry.GetCenterWallFrameOffset(
        target.X % 3,
        target.Y % 3);
    }

    LegacyWallFrameOffset frame = LegacyWallFrameLookupRegistry.GetWallFrame(
      frameLookupIndex,
      frameNumber);
    WorldTile projected = current with
    {
      WallType = wallType,
      WallFrameNumber = (byte)frameNumber,
      WallFrameX = frame.FrameX,
      WallFrameY = frame.FrameY
    };
    return new LegacyWallFrameEvaluationResult(
      true,
      target,
      projected,
      neighborMask,
      frameLookupIndex,
      randomDrawCount,
      false,
      null);
  }

  private static LegacyWallFrameEvaluationResult CreateSkippedResult(
    WallFrameCoordinate target,
    string reason)
  {
    return new LegacyWallFrameEvaluationResult(
      false,
      target,
      default,
      0,
      -1,
      0,
      false,
      reason);
  }
}
