using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyWallFrameCommandCommitSystem
{
  public bool TryCommit(
    WorldGrid world,
    IReadOnlyCollection<LegacyWallFrameCommand> commands,
    out LegacyWallFrameCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(commands);
    List<LegacyWallFrameCommand> orderedCommands = new(commands);
    HashSet<long> sequences = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      LegacyWallFrameCommand command = orderedCommands[index];
      if (command.Sequence < 0 || command.Sequence == long.MaxValue)
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command sequence was outside the valid range.");
        return false;
      }

      if (!sequences.Add(command.Sequence))
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command sequence was repeated.");
        return false;
      }

      if (!world.Contains(command.X, command.Y))
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command was outside the world.");
        return false;
      }

      if (string.IsNullOrWhiteSpace(command.Source) ||
          command.SourceLine <= 0 ||
          !MatchesExpectedSectionVersion(
            world,
            command.X,
            command.Y,
            command.ExpectedSectionVersion))
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command metadata was invalid.");
        return false;
      }

      if (command.Result.WallType >= LegacyLargeFrameWallRegistry.WallTypeCount ||
          command.Result.WallFrameNumber >= LegacyWallFrameLookupRegistry.FrameNumberCount ||
          command.NeighborMask < 0 || command.NeighborMask > 15 ||
          command.FrameLookupIndex < -1 ||
          command.FrameLookupIndex >= LegacyWallFrameLookupRegistry.MaskCount)
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command value was outside the source lookup domain.");
        return false;
      }

      if (command.Result.WallType == 0 && command.FrameLookupIndex != -1)
      {
        result = LegacyWallFrameCommitResult.Failed(
          "A zero-wall command cannot carry a frame lookup index.");
        return false;
      }

      if (command.Result.WallType != 0 && command.FrameLookupIndex < 0)
      {
        result = LegacyWallFrameCommitResult.Failed(
          "A non-zero wall command requires a frame lookup index.");
        return false;
      }

      if (command.Result.WallType == 0 && command.NeighborMask != 0)
      {
        result = LegacyWallFrameCommitResult.Failed(
          "A zero-wall command cannot carry a neighbor mask.");
        return false;
      }

      if (command.Result.WallType != 0)
      {
        int expectedFrameLookupIndex = command.NeighborMask;
        if (command.NeighborMask == 15)
        {
          expectedFrameLookupIndex += LegacyWallFrameLookupRegistry.GetCenterWallFrameOffset(
            command.X % 3,
            command.Y % 3);
        }

        if (command.FrameLookupIndex != expectedFrameLookupIndex)
        {
          result = LegacyWallFrameCommitResult.Failed(
            "Wall-frame command mask did not match its lookup index.");
          return false;
        }

        LegacyWallFrameOffset expectedFrame = LegacyWallFrameLookupRegistry.GetWallFrame(
          command.FrameLookupIndex,
          command.Result.WallFrameNumber);
        if (command.Result.WallFrameX != expectedFrame.FrameX ||
            command.Result.WallFrameY != expectedFrame.FrameY)
        {
          result = LegacyWallFrameCommitResult.Failed(
            "Wall-frame command coordinates did not match the source lookup.");
          return false;
        }
      }
    }

    orderedCommands.Sort(LegacyWallFrameCommandComparer.Instance);
    if (orderedCommands.Count != 0 && orderedCommands[^1].Sequence >= long.MaxValue - 1)
    {
      result = LegacyWallFrameCommitResult.Failed(
        "Wall-frame command sequence has no successor.");
      return false;
    }

    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    Dictionary<WorldSectionCoordinates, int> versionDeltas = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      LegacyWallFrameCommand command = orderedCommands[index];
      (int X, int Y) key = (command.X, command.Y);
      WorldTile current = projectedTiles.TryGetValue(key, out WorldTile projected)
        ? projected
        : world.GetTile(command.X, command.Y);
      if (current != command.Result)
      {
        WorldSectionCoordinates section = world.GetSectionCoordinates(command.X, command.Y);
        int delta = versionDeltas.TryGetValue(section, out int existing)
          ? checked(existing + 1)
          : 1;
        if (world.GetSectionVersion(section) > long.MaxValue - delta)
        {
          result = LegacyWallFrameCommitResult.Failed(
            "Wall-frame command would exhaust a section version.");
          return false;
        }

        versionDeltas[section] = delta;
      }

      projectedTiles[key] = command.Result;
    }

    for (int index = 0; index < orderedCommands.Count; index++)
    {
      LegacyWallFrameCommand command = orderedCommands[index];
      if (!world.TrySetTile(command.X, command.Y, command.Result))
      {
        result = LegacyWallFrameCommitResult.Failed(
          "Wall-frame command could not be applied.");
        return false;
      }
    }

    long nextSequence = orderedCommands.Count == 0
      ? 0
      : orderedCommands[^1].Sequence + 1;
    result = new LegacyWallFrameCommitResult(
      true,
      orderedCommands.Count,
      nextSequence,
      null);
    return true;
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

  private sealed class LegacyWallFrameCommandComparer : IComparer<LegacyWallFrameCommand>
  {
    public static readonly LegacyWallFrameCommandComparer Instance = new();

    public int Compare(LegacyWallFrameCommand first, LegacyWallFrameCommand second)
    {
      int priority = first.Priority.CompareTo(second.Priority);
      if (priority != 0)
      {
        return priority;
      }

      int source = StringComparer.Ordinal.Compare(first.Source, second.Source);
      if (source != 0)
      {
        return source;
      }

      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int x = first.X.CompareTo(second.X);
      return x != 0 ? x : first.Y.CompareTo(second.Y);
    }
  }
}
