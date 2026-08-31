using System;
using System.Collections.Generic;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class LiquidChangeCommitSystem
{
  public bool TryCommit(
    WorldGrid world,
    IReadOnlyCollection<PipelineLiquidChangeCommand> commands,
    IReadOnlyCollection<LiquidDefinition> definitions,
    out LiquidChangeCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(commands);
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<byte, LiquidDefinition> byType = new();
    foreach (LiquidDefinition definition in definitions)
    {
      if (!byType.TryAdd(definition.Type, definition))
      {
        result = LiquidChangeCommitResult.Failed("Liquid type was defined more than once.");
        return false;
      }
    }

    List<PipelineLiquidChangeCommand> orderedCommands = new(commands);
    HashSet<long> sequences = new();
    for (int index = 0; index < orderedCommands.Count; index++)
    {
      PipelineLiquidChangeCommand command = orderedCommands[index];
      if (command.Sequence < 0 || !sequences.Add(command.Sequence))
      {
        result = LiquidChangeCommitResult.Failed(
          "Liquid command sequence was invalid or repeated.");
        return false;
      }

      if (command.Sequence == long.MaxValue || string.IsNullOrWhiteSpace(command.Source) ||
          !MatchesExpectedSectionVersion(world, command.X, command.Y, command.ExpectedSectionVersion))
      {
        result = LiquidChangeCommitResult.Failed("Liquid command metadata was invalid.");
        return false;
      }

      if (!world.Contains(command.X, command.Y))
      {
        result = LiquidChangeCommitResult.Failed("Liquid command was outside the world.");
        return false;
      }

      if (!byType.TryGetValue(command.Type, out LiquidDefinition definition) ||
          command.Amount > definition.MaxAmount)
      {
        result = LiquidChangeCommitResult.Failed("Liquid type or amount was invalid.");
        return false;
      }
    }

    if (orderedCommands.Count != 0 && orderedCommands[^1].Sequence >= long.MaxValue - 1)
    {
      result = LiquidChangeCommitResult.Failed("Liquid command sequence has no successor.");
      return false;
    }

    orderedCommands.Sort(static (first, second) =>
    {
      int sequenceComparison = first.Sequence.CompareTo(second.Sequence);
      if (sequenceComparison != 0)
      {
        return sequenceComparison;
      }

      int xComparison = first.X.CompareTo(second.X);
      return xComparison != 0
        ? xComparison
        : first.Y.CompareTo(second.Y);
    });
    foreach (PipelineLiquidChangeCommand command in orderedCommands)
    {
      if (!world.TrySetLiquid(command.X, command.Y, command.Amount, command.Type))
      {
        result = LiquidChangeCommitResult.Failed("Liquid command could not be applied.");
        return false;
      }
    }

    result = new LiquidChangeCommitResult(true, orderedCommands.Count, null);
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
}

public sealed record LiquidChangeCommitResult(
  bool Succeeded,
  int AppliedCount,
  string? FailureReason)
{
  public static LiquidChangeCommitResult Failed(string reason)
  {
    return new LiquidChangeCommitResult(false, 0, reason);
  }
}
