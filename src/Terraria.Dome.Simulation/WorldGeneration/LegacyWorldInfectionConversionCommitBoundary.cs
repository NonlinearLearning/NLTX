using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldInfectionConversionCommitBoundary
{
  private const ushort AdjacentGrassTargetTileType = 109;
  private const ushort GrassCleanupSourceTileType = 59;
  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.RegisterDefaults();

  public static bool TryCommit(
    WorldGrid world,
    WorldGridSnapshot sourceSnapshot,
    IReadOnlyCollection<LegacyWorldInfectionConversionCommand> conversionCommands,
    ref WorldGenerationStateComponent state,
    out LegacyWorldInfectionConversionCommitResult result)
  {
    return TryCommit(
      world,
      sourceSnapshot,
      conversionCommands,
      DefaultTileDefinitions,
      ref state,
      out result);
  }

  public static bool TryCommit(
    WorldGrid world,
    WorldGridSnapshot sourceSnapshot,
    IReadOnlyCollection<LegacyWorldInfectionConversionCommand> conversionCommands,
    TileDefinitionRegistry tileDefinitions,
    ref WorldGenerationStateComponent state,
    out LegacyWorldInfectionConversionCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(sourceSnapshot);
    ArgumentNullException.ThrowIfNull(conversionCommands);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (world.Width != sourceSnapshot.Metadata.Width ||
        world.Height != sourceSnapshot.Metadata.Height)
    {
      result = LegacyWorldInfectionConversionCommitResult.Failed(
        "World dimensions did not match the conversion source snapshot.");
      return false;
    }

    List<LegacyWorldInfectionConversionCommand> orderedCommands =
      new(conversionCommands);
    orderedCommands.Sort(ConversionCommandComparer.Instance);
    HashSet<long> intentSequences = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      LegacyWorldInfectionConversionCommand command = orderedCommands[index];
      if (!command.IsValid(sourceSnapshot))
      {
        result = LegacyWorldInfectionConversionCommitResult.Failed(
          "World infection conversion command failed immutable validation.");
        return false;
      }

      if (!intentSequences.Add(command.Sequence))
      {
        result = LegacyWorldInfectionConversionCommitResult.Failed(
          "World infection conversion intent sequence was repeated.");
        return false;
      }

      WorldSectionCoordinates coordinates = world.GetSectionCoordinates(command.X, command.Y);
      if (world.GetSectionVersion(coordinates) != sourceSnapshot.GetSectionVersion(coordinates) ||
          world.GetTile(command.X, command.Y) != sourceSnapshot.GetTile(command.X, command.Y))
      {
        result = LegacyWorldInfectionConversionCommitResult.Failed(
          "World infection conversion source snapshot was stale.");
        return false;
      }
    }

    WorldGenerationStateComponent originalState = state;
    List<TileChangeCommand> tileCommands = new();
    List<LegacyWorldInfectionConversionDeferred> deferred = new();
    int appliedTileCount = 0;
    int appliedWallCount = 0;
    int noOpCount = 0;
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      LegacyWorldInfectionConversionCommand command = orderedCommands[index];
      WorldTile sourceTile = sourceSnapshot.GetTile(command.X, command.Y);
      if (command.ConvertWalls)
      {
        if (command.ConversionType is 8 or 9 or 10)
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Wall,
            LegacyWorldInfectionConversionDeferredReason.UnsupportedChannel,
            sourceTile.WallType);
        }
        else if (sourceTile.WallType == 0)
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Wall,
            LegacyWorldInfectionConversionDeferredReason.EmptyWall,
            sourceTile.WallType);
        }
        else if (LegacyWorldInfectionConversionRegistry.TryGetWallRule(
                   command.ConversionType,
                   sourceTile.WallType,
                   out LegacyWorldInfectionConversionRule wallRule))
        {
          if (wallRule.IsDeferred)
          {
            AddDeferred(
              deferred,
              command,
              LegacyWorldInfectionConversionChannel.Wall,
              LegacyWorldInfectionConversionDeferredReason.ConversionRuleDeferred,
              sourceTile.WallType);
          }
          else if (wallRule.TargetType == sourceTile.WallType)
          {
            noOpCount++;
          }
          else
          {
            if (!TryReserveCommandSequence(ref state, out long sequence))
            {
              state = originalState;
              result = LegacyWorldInfectionConversionCommitResult.Failed(
                "World infection conversion command sequence had no successor.");
              return false;
            }

            tileCommands.Add(new TileChangeCommand(
              sequence,
              command.X,
              command.Y,
              TileChangeKind.SetWall,
              0,
              WallType: wallRule.TargetType,
              Source: command.Source));
            appliedWallCount++;
          }
        }
        else if (LegacyWorldInfectionConversionRegistry.IsWallTarget(
                   command.ConversionType,
                   sourceTile.WallType))
        {
          noOpCount++;
        }
        else
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Wall,
            LegacyWorldInfectionConversionDeferredReason.WallCategoryNotRegistered,
            sourceTile.WallType);
        }
      }

      if (command.ConvertTiles)
      {
        if (!sourceTile.IsActive)
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Tile,
            LegacyWorldInfectionConversionDeferredReason.InactiveTile,
            sourceTile.Type);
        }
        else if (command.ConversionType == 2 && sourceTile.Type == GrassCleanupSourceTileType &&
                 HasAdjacentTileType(sourceSnapshot, command.X, command.Y,
                   AdjacentGrassTargetTileType))
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Tile,
            LegacyWorldInfectionConversionDeferredReason.AdjacentGrassCleanup,
            sourceTile.Type);
        }
        else if (command.ConversionType == 2 && sourceTile.Type == GrassCleanupSourceTileType)
        {
          noOpCount++;
        }
        else if (LegacyWorldInfectionConversionRegistry.TryGetTileRule(
                   command.ConversionType,
                   sourceTile.Type,
                   out LegacyWorldInfectionConversionRule tileRule))
        {
          if (tileRule.TargetType == sourceTile.Type)
          {
            noOpCount++;
          }
          else if (tileRule.IsDeferred)
          {
            LegacyWorldInfectionConversionDeferredReason reason = tileRule.Category switch
            {
              LegacyWorldInfectionConversionCategory.Torch =>
                LegacyWorldInfectionConversionDeferredReason.TorchFrameMutation,
              LegacyWorldInfectionConversionCategory.Thorn =>
                LegacyWorldInfectionConversionDeferredReason.ThornKillMutation,
              LegacyWorldInfectionConversionCategory.ChlorophyteKill =>
                LegacyWorldInfectionConversionDeferredReason.ChlorophyteKillMutation,
              _ => LegacyWorldInfectionConversionDeferredReason.ConversionRuleDeferred
            };
            AddDeferred(
              deferred,
              command,
              LegacyWorldInfectionConversionChannel.Tile,
              reason,
              sourceTile.Type);
          }
          else
          {
            ushort targetType = ResolveTileTarget(
              tileRule,
              sourceSnapshot,
              tileDefinitions,
              command.X,
              command.Y);
            if (tileRule.TargetType == sourceTile.Type || targetType == sourceTile.Type)
            {
              noOpCount++;
            }
            else if (!TryReserveCommandSequence(ref state, out long sequence))
            {
              state = originalState;
              result = LegacyWorldInfectionConversionCommitResult.Failed(
                "World infection conversion command sequence had no successor.");
              return false;
            }
            else
            {
              tileCommands.Add(new TileChangeCommand(
                sequence,
                command.X,
                command.Y,
                TileChangeKind.UpdateTileType,
                targetType,
                IsActive: true,
                Source: command.Source));
              appliedTileCount++;
            }
          }
        }
        else if (LegacyWorldInfectionConversionRegistry.IsTileTarget(
                   command.ConversionType,
                   sourceTile.Type))
        {
          noOpCount++;
        }
        else
        {
          AddDeferred(
            deferred,
            command,
            LegacyWorldInfectionConversionChannel.Tile,
            LegacyWorldInfectionConversionDeferredReason.TileCategoryNotRegistered,
            sourceTile.Type);
        }
      }
    }

    if (!ValidateSectionCapacity(world, tileCommands, out string? capacityFailure))
    {
      state = originalState;
      result = LegacyWorldInfectionConversionCommitResult.Failed(capacityFailure!);
      return false;
    }

    if (tileCommands.Count != 0 &&
        !new TileChangeCommitSystem().TryCommit(
          world,
          tileCommands,
          out TileChangeCommitResult tileResult))
    {
      state = originalState;
      result = LegacyWorldInfectionConversionCommitResult.Failed(
        tileResult.FailureReason ?? "World infection conversion tile commit failed.");
      return false;
    }

    result = new LegacyWorldInfectionConversionCommitResult(
      true,
      appliedTileCount,
      appliedWallCount,
      noOpCount,
      deferred.AsReadOnly(),
      null);
    return true;
  }

  private static bool TryReserveCommandSequence(
    ref WorldGenerationStateComponent state,
    out long sequence)
  {
    if (state.IsComplete || state.NextSequence >= long.MaxValue - 1)
    {
      sequence = 0;
      return false;
    }

    sequence = state.ReserveSequence();
    return true;
  }

  private static void AddDeferred(
    ICollection<LegacyWorldInfectionConversionDeferred> deferred,
    LegacyWorldInfectionConversionCommand command,
    LegacyWorldInfectionConversionChannel channel,
    LegacyWorldInfectionConversionDeferredReason reason,
    ushort sourceType)
  {
    deferred.Add(new LegacyWorldInfectionConversionDeferred(
      command.Sequence,
      command.X,
      command.Y,
      command.ConversionType,
      channel,
      reason,
      sourceType));
  }

  private static ushort ResolveTileTarget(
    LegacyWorldInfectionConversionRule rule,
    WorldGridSnapshot sourceSnapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    if (rule.Category == LegacyWorldInfectionConversionCategory.GrassSandSnowDirt &&
        SandFallEligibilityQuery.BlockBelowMakesSandConvertIntoHardenedSand(
          sourceSnapshot,
          tileDefinitions,
          x,
          y))
    {
      return 397;
    }

    return rule.TargetType;
  }

  private static bool HasAdjacentTileType(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort tileType)
  {
    return IsTileType(snapshot, x - 1, y, tileType) ||
      IsTileType(snapshot, x + 1, y, tileType) ||
      IsTileType(snapshot, x, y - 1, tileType) ||
      IsTileType(snapshot, x, y + 1, tileType);
  }

  private static bool IsTileType(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort tileType)
  {
    return snapshot.Metadata.IsInside(x, y) &&
      snapshot.GetTile(x, y).Type == tileType;
  }

  private static bool ValidateSectionCapacity(
    WorldGrid world,
    IReadOnlyCollection<TileChangeCommand> commands,
    out string? failureReason)
  {
    List<TileChangeCommand> orderedCommands = new(commands);
    orderedCommands.Sort(TileChangeCommandComparer.Instance);
    Dictionary<WorldSectionCoordinates, int> versionDeltas = new();
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      TileChangeCommand command = orderedCommands[index];
      (int X, int Y) coordinate = (command.X, command.Y);
      WorldTile current = projectedTiles.TryGetValue(coordinate, out WorldTile projected)
        ? projected
        : world.GetTile(command.X, command.Y);
      WorldTile next = TileMutationProjection.Apply(current, command);
      projectedTiles[coordinate] = next;
      if (next == current)
      {
        continue;
      }

      WorldSectionCoordinates section = world.GetSectionCoordinates(command.X, command.Y);
      int delta = versionDeltas.TryGetValue(section, out int existing) ? existing + 1 : 1;
      if (world.GetSectionVersion(section) > long.MaxValue - delta)
      {
        failureReason = "World infection conversion would exhaust a section version.";
        return false;
      }

      versionDeltas[section] = delta;
    }

    failureReason = null;
    return true;
  }

  private sealed class ConversionCommandComparer : IComparer<LegacyWorldInfectionConversionCommand>
  {
    public static readonly ConversionCommandComparer Instance = new();

    public int Compare(
      LegacyWorldInfectionConversionCommand first,
      LegacyWorldInfectionConversionCommand second)
    {
      int sequenceComparison = first.Sequence.CompareTo(second.Sequence);
      if (sequenceComparison != 0)
      {
        return sequenceComparison;
      }

      int xComparison = first.X.CompareTo(second.X);
      return xComparison != 0 ? xComparison : first.Y.CompareTo(second.Y);
    }
  }

  private sealed class TileChangeCommandComparer : IComparer<TileChangeCommand>
  {
    public static readonly TileChangeCommandComparer Instance = new();

    public int Compare(TileChangeCommand first, TileChangeCommand second)
    {
      int priorityComparison = first.Priority.CompareTo(second.Priority);
      if (priorityComparison != 0)
      {
        return priorityComparison;
      }

      int sourceComparison = StringComparer.Ordinal.Compare(first.Source, second.Source);
      if (sourceComparison != 0)
      {
        return sourceComparison;
      }

      int sequenceComparison = first.Sequence.CompareTo(second.Sequence);
      if (sequenceComparison != 0)
      {
        return sequenceComparison;
      }

      int xComparison = first.X.CompareTo(second.X);
      if (xComparison != 0)
      {
        return xComparison;
      }

      int yComparison = first.Y.CompareTo(second.Y);
      return yComparison != 0 ? yComparison : first.Kind.CompareTo(second.Kind);
    }
  }
}
