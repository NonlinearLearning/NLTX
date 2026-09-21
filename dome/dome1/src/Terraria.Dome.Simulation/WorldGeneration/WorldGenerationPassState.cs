using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>
/// Restartable state for a single ordered generation pass.
/// </summary>
public sealed record WorldGenerationPassState
{
  public WorldGenerationPassState(
    WorldGenerationStage stage,
    TileReadSnapshot readSnapshot,
    GenerationRandomState random,
    long cursor,
    WorldGenerationCommandBatch? commands = null,
    string? failureReason = null)
  {
    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    ArgumentNullException.ThrowIfNull(readSnapshot);
    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    Stage = stage;
    ReadSnapshot = readSnapshot;
    Random = random;
    Cursor = cursor;
    Commands = commands ?? new WorldGenerationCommandBatch();
    FailureReason = failureReason;
  }

  public WorldGenerationStage Stage { get; }

  public TileReadSnapshot ReadSnapshot { get; }

  public GenerationRandomState Random { get; }

  public long Cursor { get; }

  public WorldGenerationCommandBatch Commands { get; }

  public string? FailureReason { get; }

  public WorldGenerationPassState WithCheckpoint(
    long cursor,
    GenerationRandomState random,
    WorldGenerationCommandBatch commands)
  {
    return new WorldGenerationPassState(
      Stage,
      ReadSnapshot,
      random,
      cursor,
      commands,
      FailureReason);
  }

  public WorldGenerationPassState Failed(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new WorldGenerationPassState(
      Stage,
      ReadSnapshot,
      Random,
      Cursor,
      Commands,
      reason);
  }
}
