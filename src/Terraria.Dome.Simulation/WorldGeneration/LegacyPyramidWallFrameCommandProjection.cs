using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidWallFrameCommandProjection
{
  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    IReadOnlyList<LegacyPyramidWallFrameRequest> requests,
    bool showInvisibleWalls,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<LegacyWallFrameCommand> commands,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(requests);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    failureReason = null;
    if (state.IsComplete)
    {
      failureReason = "Pyramid wall-frame projection received a completed generation state.";
      return false;
    }

    if (state.Stage > WorldGenerationStage.Framing)
    {
      failureReason = "Pyramid wall-frame projection received a post-framing generation state.";
      return false;
    }

    if (!TryGetMaximumCount(requests.Count, out long maximumCount) ||
        maximumCount > 0 && state.NextSequence > long.MaxValue - 1 - maximumCount)
    {
      failureReason = "Pyramid wall-frame command sequence capacity was exhausted.";
      return false;
    }

    if (!TryGetMaximumRandomDraws(requests.Count, out long maximumRandomDraws) ||
        random.SampleCount > long.MaxValue - maximumRandomDraws)
    {
      failureReason = "Pyramid wall-frame random sample capacity was exhausted.";
      return false;
    }

    for (int index = 0; index < requests.Count; index++)
    {
      LegacyPyramidWallFrameRequest request = requests[index];
      if (!StringComparer.Ordinal.Equals(
            request.Source,
            LegacyPyramidWallFrameRequestQuery.Source) ||
          request.SourceLine != LegacyPyramidWallFrameRequestQuery.SourceLine ||
          !request.ResetFrame ||
          !HasInsideNeighborhood(snapshot.Metadata, request.Center) ||
          !IsStrictInterior(snapshot.Metadata, request.Target) ||
          !IsNeighborTarget(request.Center, request.Target))
      {
        failureReason = "Pyramid wall-frame request metadata or target was invalid.";
        return false;
      }
    }

    if (requests.Count == 0)
    {
      return true;
    }

    WorldGenerationStateComponent workingState = state;
    if (workingState.Stage < WorldGenerationStage.Framing &&
        !workingState.TryAdvance(WorldGenerationStage.Framing))
    {
      failureReason = "Pyramid wall-frame projection could not enter the framing stage.";
      return false;
    }

    List<LegacyWallFrameCommand> pending = new(requests.Count);
    for (int index = 0; index < requests.Count; index++)
    {
      LegacyPyramidWallFrameRequest request = requests[index];
      LegacyWallFrameEvaluationResult evaluation = LegacyWallFrameEvaluationQuery.Evaluate(
        snapshot,
        request.Target,
        request.ResetFrame,
        showInvisibleWalls,
        random);
      if (!evaluation.IsApplicable)
      {
        continue;
      }

      WorldSectionCoordinates section = snapshot.Metadata.IsInside(
          request.Target.X,
          request.Target.Y)
        ? new WorldSectionCoordinates(
          request.Target.X / WorldGrid.SectionWidth,
          request.Target.Y / WorldGrid.SectionHeight)
        : default;
      pending.Add(new LegacyWallFrameCommand(
        workingState.ReserveSequence(),
        request.Target.X,
        request.Target.Y,
        evaluation.Projected,
        evaluation.NeighborMask,
        evaluation.FrameLookupIndex,
        request.ResetFrame,
        request.Source,
        request.SourceLine,
        ExpectedSectionVersion: snapshot.GetSectionVersion(section)));
    }

    commands.AddRange(pending);
    state = workingState;
    return true;
  }

  private static bool TryGetMaximumCount(int requestCount, out long maximumCount)
  {
    maximumCount = requestCount;
    return requestCount >= 0;
  }

  private static bool TryGetMaximumRandomDraws(int requestCount, out long maximumRandomDraws)
  {
    try
    {
      maximumRandomDraws = checked((long)requestCount * 2L);
      return true;
    }
    catch (OverflowException)
    {
      maximumRandomDraws = 0;
      return false;
    }
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

  private static bool IsStrictInterior(
    WorldMetadata metadata,
    WallFrameCoordinate target)
  {
    return target.X > 0 &&
      target.X < metadata.Width - 1 &&
      target.Y > 0 &&
      target.Y < metadata.Height - 1;
  }

  private static bool IsNeighborTarget(
    WallFrameCoordinate center,
    WallFrameCoordinate target)
  {
    long offsetX = (long)target.X - center.X;
    long offsetY = (long)target.Y - center.Y;
    return offsetX >= -1 && offsetX <= 1 && offsetY >= -1 && offsetY <= 1;
  }
}
