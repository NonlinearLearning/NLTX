using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class TileChangeCommitSystem
{
  private const ushort CartTrackTileType = 314;

  public bool TryCommit(
    WorldGrid world,
    IReadOnlyCollection<TileFrameCommand> commands,
    out TileFrameCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(commands);
    List<TileFrameCommand> orderedCommands = new(commands);
    HashSet<long> sequences = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      TileFrameCommand command = orderedCommands[index];
      if (command.Sequence < 0 || command.Sequence == long.MaxValue)
      {
        result = TileFrameCommitResult.Failed("Tile frame command sequence cannot be negative.");
        return false;
      }

      if (!sequences.Add(command.Sequence))
      {
        result = TileFrameCommitResult.Failed("Tile frame command sequence was repeated.");
        return false;
      }

      if (!world.Contains(command.X, command.Y))
      {
        result = TileFrameCommitResult.Failed("Tile frame command was outside the world.");
        return false;
      }

      if (string.IsNullOrWhiteSpace(command.Source) ||
          !MatchesExpectedSectionVersion(world, command.X, command.Y, command.ExpectedSectionVersion))
      {
        result = TileFrameCommitResult.Failed("Tile frame command metadata was invalid.");
        return false;
      }
    }

    orderedCommands.Sort(TileFrameCommandComparer.Instance);
    if (orderedCommands.Count != 0 && orderedCommands[^1].Sequence >= long.MaxValue - 1)
    {
      result = TileFrameCommitResult.Failed("Tile frame command sequence has no successor.");
      return false;
    }

    orderedCommands.Sort(TileFrameCommandComparer.Instance);
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      TileFrameCommand command = orderedCommands[index];
      WorldTile current = world.GetTile(command.X, command.Y);
      WorldTile tile = current with
      {
        FrameX = command.FrameX,
        FrameY = command.FrameY,
        IsHalfBrick = command.IsHalfBrick ?? current.IsHalfBrick,
        Slope = command.Slope ?? current.Slope
      };
      if (!world.TrySetTile(command.X, command.Y, tile))
      {
        result = TileFrameCommitResult.Failed("Tile frame command could not be applied.");
        return false;
      }
    }

    long nextSequence = orderedCommands.Count == 0
      ? 0
      : orderedCommands[^1].Sequence + 1;
    result = new TileFrameCommitResult(true, orderedCommands.Count, nextSequence, null);
    return true;
  }

  public bool TryCommit(
    WorldGrid world,
    IReadOnlyCollection<TileChangeCommand> commands,
    out TileChangeCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(commands);
    List<TileChangeCommand> orderedCommands = new(commands);
    HashSet<long> sequences = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      TileChangeCommand command = orderedCommands[index];
      if (command.Sequence < 0 || command.Sequence == long.MaxValue)
      {
        result = TileChangeCommitResult.Failed("Tile command sequence cannot be negative.");
        return false;
      }

      if (!sequences.Add(command.Sequence))
      {
        result = TileChangeCommitResult.Failed("Tile command sequence was repeated.");
        return false;
      }

      if (!world.Contains(command.X, command.Y))
      {
        result = TileChangeCommitResult.Failed("Tile command was outside the world.");
        return false;
      }

      if (string.IsNullOrWhiteSpace(command.Source) ||
          !MatchesExpectedSectionVersion(world, command.X, command.Y, command.ExpectedSectionVersion))
      {
        result = TileChangeCommitResult.Failed("Tile command metadata was invalid.");
        return false;
      }

      if (!Enum.IsDefined(command.Kind))
      {
        result = TileChangeCommitResult.Failed("Tile command kind was invalid.");
        return false;
      }

      if (command.IsCartTrack &&
          (command.Kind is not (TileChangeKind.Place or TileChangeKind.PlaceTile) ||
           command.TileType != CartTrackTileType || command.WallType != 0))
      {
        result = TileChangeCommitResult.Failed(
          $"CartTrack tile commands require tile type {CartTrackTileType} placement " +
          "without a wall.");
        return false;
      }
    }

    orderedCommands.Sort(TileChangeCommandComparer.Instance);
    if (orderedCommands.Count != 0 && orderedCommands[^1].Sequence >= long.MaxValue - 1)
    {
      result = TileChangeCommitResult.Failed("Tile command sequence has no successor.");
      return false;
    }

    orderedCommands.Sort(TileChangeCommandComparer.Instance);
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      TileChangeCommand command = orderedCommands[index];
      WorldTile tile = TileMutationProjection.Apply(
        world.GetTile(command.X, command.Y),
        command);
      if (!world.TrySetTile(command.X, command.Y, tile))
      {
        result = TileChangeCommitResult.Failed("Tile command could not be applied.");
        return false;
      }
    }

    long nextSequence = orderedCommands.Count == 0
      ? 0
      : orderedCommands[^1].Sequence + 1;
    result = new TileChangeCommitResult(true, orderedCommands.Count, nextSequence, null);
    return true;
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

  private sealed class TileFrameCommandComparer : IComparer<TileFrameCommand>
  {
    public static readonly TileFrameCommandComparer Instance = new();

    public int Compare(TileFrameCommand first, TileFrameCommand second)
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
      if (yComparison != 0)
      {
        return yComparison;
      }

      int frameXComparison = first.FrameX.CompareTo(second.FrameX);
      return frameXComparison != 0
        ? frameXComparison
        : first.FrameY.CompareTo(second.FrameY);
    }
  }

  private static bool MatchesExpectedSectionVersion(
    WorldGrid world,
    int x,
    int y,
    long? expectedSectionVersion)
  {
    return !expectedSectionVersion.HasValue ||
      world.GetSectionVersion(world.GetSectionCoordinates(x, y)) == expectedSectionVersion.Value;
  }
}

public sealed record TileChangeCommitResult(
  bool Succeeded,
  int AppliedCount,
  long NextSequence,
  string? FailureReason)
{
  public static TileChangeCommitResult Failed(string reason)
  {
    return new TileChangeCommitResult(false, 0, 0, reason);
  }
}

public sealed record TileFrameCommitResult(
  bool Succeeded,
  int AppliedCount,
  long NextSequence,
  string? FailureReason)
{
  public static TileFrameCommitResult Failed(string reason)
  {
    return new TileFrameCommitResult(false, 0, 0, reason);
  }
}
