using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyCavePassResetCheckpoint(
  string PassName,
  int PassIndex,
  int WorldSeed,
  int ResetOrdinal);

public sealed class LegacyCavePassExecutionCoordinator
{
  private readonly IReadOnlyList<LegacyCavePassDefinition> _schedule;
  private readonly int _worldSeed;
  private int _nextPassIndex;

  public LegacyCavePassExecutionCoordinator(
    IReadOnlyList<LegacyCavePassDefinition> schedule,
    int worldSeed)
  {
    ArgumentNullException.ThrowIfNull(schedule);
    if (schedule.Count == 0)
    {
      throw new ArgumentException("Cave pass schedule cannot be empty.", nameof(schedule));
    }

    _schedule = schedule;
    _worldSeed = worldSeed;
  }

  public int CompletedPassCount => _nextPassIndex;

  public LegacyCavePassResetCheckpoint BeginNextPass(out LegacyPassRandomState random)
  {
    if (_nextPassIndex >= _schedule.Count)
    {
      throw new InvalidOperationException("Cave pass schedule was already exhausted.");
    }

    LegacyCavePassDefinition definition = _schedule[_nextPassIndex];
    if (!definition.ResetsRandomFromWorldSeed)
    {
      throw new InvalidOperationException(
        "Cave pass lacks the required world-seed random reset contract.");
    }

    int index = _nextPassIndex;
    _nextPassIndex++;
    random = new LegacyPassRandomState(_worldSeed);
    return new LegacyCavePassResetCheckpoint(
      definition.Name,
      index,
      _worldSeed,
      index + 1);
  }

  public void Complete()
  {
    if (_nextPassIndex != _schedule.Count)
    {
      throw new InvalidOperationException(
        "Cave pass execution completed before every scheduled pass ran.");
    }
  }
}
