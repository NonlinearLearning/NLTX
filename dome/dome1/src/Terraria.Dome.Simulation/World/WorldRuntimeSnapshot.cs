using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldRuntimeSnapshot
{
  public WorldRuntimeSnapshot(
    WorldClockSnapshot clock,
    WorldRuleState rules,
    bool generationCompleted,
    long generationCompletionTick)
  {
    ArgumentNullException.ThrowIfNull(rules);
    if (!generationCompleted && generationCompletionTick != -1)
    {
      throw new ArgumentException(
        "An incomplete generation cannot carry a completion tick.",
        nameof(generationCompletionTick));
    }

    if (generationCompleted && generationCompletionTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationCompletionTick));
    }

    if (generationCompleted && generationCompletionTick > clock.TickNumber)
    {
      throw new ArgumentException(
        "Generation cannot complete after the runtime clock.",
        nameof(generationCompletionTick));
    }

    Clock = clock;
    Rules = rules;
    GenerationCompleted = generationCompleted;
    GenerationCompletionTick = generationCompletionTick;
  }

  public WorldClockSnapshot Clock { get; }

  public long GenerationCompletionTick { get; }

  public bool GenerationCompleted { get; }

  public WorldRuleState Rules { get; }

  public WorldRuntimeSnapshot CompleteGeneration()
  {
    if (GenerationCompleted)
    {
      return this;
    }

    return new WorldRuntimeSnapshot(
      Clock,
      Rules,
      generationCompleted: true,
      generationCompletionTick: Clock.TickNumber);
  }
}
