using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldShufflePass
{
  private const int MinimumX = 30;
  private const int MaximumXMargin = 30;
  private const int MinimumY = 80;
  private const int MaximumYMargin = 30;

  public static LegacyErrorWorldShuffleResult AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyErrorWorldTileDefinitionRegistry registry,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    LegacyErrorWorldPassCounts counts,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(registry);
    return AppendCommands(
      snapshot,
      registry.ByType,
      registry.Definitions,
      exclusionProfile,
      counts,
      isSkyblockWorld,
      random,
      ref state,
      commands);
  }

  public static LegacyErrorWorldShuffleResult AppendCommands(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    IReadOnlyList<LegacyErrorWorldTileDefinition> replacementDefinitions,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    LegacyErrorWorldPassCounts counts,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(replacementDefinitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    int randomBlockRewrites = AppendRandomBlockRewrites(
      snapshot, definitions, replacementDefinitions, counts.RandomBlockRewrites,
      isSkyblockWorld, random, ref state, commands);
    int singleTileSwaps = AppendSingleTileSwaps(
      snapshot, definitions, exclusionProfile, counts.SingleTileSwaps, random, ref state, commands);
    int rectangleSwaps = AppendRectangleSwaps(
      snapshot, definitions, exclusionProfile, counts.RectangleSwaps, random, ref state, commands);
    int axisTrails = AppendAxisTrails(
      snapshot, definitions, counts.AxisTrails, random, ref state, commands);
    return new LegacyErrorWorldShuffleResult(
      randomBlockRewrites,
      singleTileSwaps,
      rectangleSwaps,
      axisTrails,
      randomBlockRewrites == counts.RandomBlockRewrites &&
      singleTileSwaps == counts.SingleTileSwaps &&
      rectangleSwaps == counts.RectangleSwaps &&
      axisTrails == counts.AxisTrails);
  }

  private static int AppendRandomBlockRewrites(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    IReadOnlyList<LegacyErrorWorldTileDefinition> replacementDefinitions,
    int count,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int completed = 0;
    for (int index = 0; index < count; index++)
    {
      if (!LegacyErrorWorldRandomBlockRewrite.TryAppendCommand(
            snapshot,
            definitions,
            replacementDefinitions,
            MinimumX,
            snapshot.Metadata.Width - MaximumXMargin,
            MinimumY,
            snapshot.Metadata.Height - MaximumYMargin,
            isSkyblockWorld,
            random,
            ref state,
            commands))
      {
        break;
      }

      completed++;
    }

    return completed;
  }

  private static int AppendSingleTileSwaps(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    int count,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int completed = 0;
    for (int index = 0; index < count; index++)
    {
      if (!LegacyErrorWorldSwapOperation.TryAppendSingleTileSwap(
            snapshot,
            definitions,
            exclusionProfile,
            MinimumX,
            snapshot.Metadata.Width - MaximumXMargin,
            MinimumY,
            snapshot.Metadata.Height - MaximumYMargin,
            random,
            ref state,
            commands))
      {
        break;
      }

      completed++;
    }

    return completed;
  }

  private static int AppendRectangleSwaps(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    LegacyErrorWorldSpawnExclusionProfile exclusionProfile,
    int count,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int completed = 0;
    for (int index = 0; index < count; index++)
    {
      int width;
      int height;
      if (random.Next(2) == 0)
      {
        width = random.Next(5, 21);
        height = random.Next(1, 4);
      }
      else
      {
        width = random.Next(1, 4);
        height = random.Next(5, 21);
      }

      if (!LegacyErrorWorldSwapOperation.TryAppendRectangleSwap(
            snapshot,
            definitions,
            exclusionProfile,
            MinimumX,
            snapshot.Metadata.Width - MaximumXMargin,
            MinimumY,
            snapshot.Metadata.Height - MaximumYMargin,
            width,
            height,
            random,
            ref state,
            commands))
      {
        break;
      }

      completed++;
    }

    return completed;
  }

  private static int AppendAxisTrails(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int count,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int completed = 0;
    for (int index = 0; index < count; index++)
    {
      if (!LegacyErrorWorldCandidateSelector.TrySelect(
            snapshot,
            definitions,
            MinimumX,
            snapshot.Metadata.Width - MaximumXMargin,
            MinimumY,
            snapshot.Metadata.Height - MaximumYMargin,
            1,
            1,
            random,
            out LegacyErrorWorldCandidate candidate))
      {
        break;
      }

      LegacyErrorWorldAxisTrailPlan plan = LegacyErrorWorldAxisTrailPolicy.Create(random);
      _ = LegacyErrorWorldAxisTrail.AppendCommands(
        snapshot,
        candidate.X,
        candidate.Y,
        plan.Direction,
        plan.Length,
        ref state,
        commands);
      completed++;
    }

    return completed;
  }
}
