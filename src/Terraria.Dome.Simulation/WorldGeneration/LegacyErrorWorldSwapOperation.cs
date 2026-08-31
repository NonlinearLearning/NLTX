using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldSwapOperation
{
  public static bool TryAppendSingleTileSwap(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (!LegacyErrorWorldCandidateSelector.TrySelect(
          snapshot,
          definitions,
          minimumX,
          maximumXExclusive,
          minimumY,
          maximumYExclusive,
          1,
          1,
          random,
          out LegacyErrorWorldCandidate first) ||
        !LegacyErrorWorldCandidateSelector.TrySelect(
          snapshot,
          definitions,
          minimumX,
          maximumXExclusive,
          minimumY,
          maximumYExclusive,
          1,
          1,
          random,
          out LegacyErrorWorldCandidate second) ||
        LegacyErrorWorldSpawnExclusionPolicy.IsSingleTileExcluded(
          exclusionProfile, first.X, first.Y) ||
        LegacyErrorWorldSpawnExclusionPolicy.IsSingleTileExcluded(
          exclusionProfile, second.X, second.Y))
    {
      return false;
    }

    LegacyErrorWorldTileSwap.AppendCommands(
      first.X,
      first.Y,
      second.X,
      second.Y,
      snapshot.GetTile(first.X, first.Y),
      snapshot.GetTile(second.X, second.Y),
      ref state,
      commands);
    return true;
  }

  public static bool TryAppendRectangleSwap(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    int width,
    int height,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (!LegacyErrorWorldCandidateSelector.TrySelect(
          snapshot,
          definitions,
          minimumX,
          maximumXExclusive,
          minimumY,
          maximumYExclusive,
          width,
          height,
          random,
          out LegacyErrorWorldCandidate first) ||
        !LegacyErrorWorldCandidateSelector.TrySelect(
          snapshot,
          definitions,
          minimumX,
          maximumXExclusive,
          minimumY,
          maximumYExclusive,
          width,
          height,
          random,
          out LegacyErrorWorldCandidate second) ||
        RectanglesOverlap(first, second, width, height) ||
        LegacyErrorWorldSpawnExclusionPolicy.IsRectangleExcluded(
          exclusionProfile, first.X, first.Y, width) ||
        LegacyErrorWorldSpawnExclusionPolicy.IsRectangleExcluded(
          exclusionProfile, second.X, second.Y, width))
    {
      return false;
    }

    LegacyErrorWorldTileRectangleSwap.AppendCommands(
      snapshot,
      first.X,
      first.Y,
      second.X,
      second.Y,
      width,
      height,
      ref state,
      commands);
    return true;
  }

  private static bool RectanglesOverlap(
    LegacyErrorWorldCandidate first,
    LegacyErrorWorldCandidate second,
    int width,
    int height)
  {
    return first.X < second.X + width && second.X < first.X + width &&
      first.Y < second.Y + height && second.Y < first.Y + height;
  }
}
