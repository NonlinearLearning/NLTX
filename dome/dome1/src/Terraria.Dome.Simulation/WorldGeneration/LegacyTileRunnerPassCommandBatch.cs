using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassCommandBatch(
  string PassName,
  IReadOnlyList<TileChangeCommand> TileCommands,
  IReadOnlyList<LiquidChangeCommand> LiquidCommands,
  long NextSequence);

public static class LegacyTileRunnerPassCommandBatchFactory
{
  public static LegacyTileRunnerPassCommandBatch Create(
    string passName,
    IReadOnlyCollection<LegacyTileRunnerCommandBatch> batches,
    long startingSequence = 0)
  {
    ArgumentException.ThrowIfNullOrEmpty(passName);
    ArgumentNullException.ThrowIfNull(batches);
    if (startingSequence < 0 || startingSequence == long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(startingSequence));
    }

    List<TileChangeCommand> tileCommands = new();
    List<LiquidChangeCommand> liquidCommands = new();
    HashSet<long> sequences = new();
    foreach (LegacyTileRunnerCommandBatch batch in batches)
    {
      if (batch is null)
      {
        throw new ArgumentException("TileRunner command batch cannot be null.", nameof(batches));
      }

      if (batch.TileCommand.HasValue)
      {
        AddTileCommand(batch.TileCommand.Value, tileCommands, sequences);
      }

      if (batch.LiquidCommand.HasValue)
      {
        AddLiquidCommand(batch.LiquidCommand.Value, liquidCommands, sequences);
      }
    }

    tileCommands.Sort(static (first, second) => first.Sequence.CompareTo(second.Sequence));
    liquidCommands.Sort(static (first, second) => first.Sequence.CompareTo(second.Sequence));
    long nextSequence = startingSequence;
    foreach (long sequence in sequences)
    {
      if (sequence < startingSequence || sequence >= long.MaxValue - 1)
      {
        throw new ArgumentOutOfRangeException(nameof(batches));
      }

      nextSequence = Math.Max(nextSequence, sequence + 1);
    }

    return new LegacyTileRunnerPassCommandBatch(
      passName,
      tileCommands.AsReadOnly(),
      liquidCommands.AsReadOnly(),
      nextSequence);
  }

  private static void AddTileCommand(
    TileChangeCommand command,
    ICollection<TileChangeCommand> commands,
    ISet<long> sequences)
  {
    ValidateSequence(command.Sequence);
    if (!sequences.Add(command.Sequence))
    {
      throw new InvalidOperationException(
        "TileRunner pass command sequence was repeated across tile and liquid commands.");
    }

    commands.Add(command);
  }

  private static void AddLiquidCommand(
    LiquidChangeCommand command,
    ICollection<LiquidChangeCommand> commands,
    ISet<long> sequences)
  {
    ValidateSequence(command.Sequence);
    if (!sequences.Add(command.Sequence))
    {
      throw new InvalidOperationException(
        "TileRunner pass command sequence was repeated across tile and liquid commands.");
    }

    commands.Add(command);
  }

  private static void ValidateSequence(long sequence)
  {
    if (sequence < 0 || sequence == long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(sequence));
    }
  }
}
