using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckActiveDeactivationPolicy
{
  public static NpcCheckActiveDeactivationDecision Evaluate(
    NpcCheckActiveDeactivationInput input)
  {
    Validate(input);
    if (!input.IsNpcActive || input.DoesNotDespawnToInactivityAndCountsNpcSlots)
    {
      return new(
        WasProcessed: false,
        IsActive: input.IsNpcActive,
        TimeLeft: input.TimeLeft,
        Life: input.Life,
        ShouldSkipNextSpawnCycle: false);
    }

    int timeLeft = input.TimeLeft - 1;
    if (input.HasKeepAlive && timeLeft > 0)
    {
      return new(
        WasProcessed: true,
        IsActive: true,
        TimeLeft: timeLeft,
        Life: input.Life,
        ShouldSkipNextSpawnCycle: false);
    }

    return new(
      WasProcessed: true,
      IsActive: false,
      TimeLeft: timeLeft,
      Life: 0,
      ShouldSkipNextSpawnCycle: true);
  }

  private static void Validate(NpcCheckActiveDeactivationInput input)
  {
    if (input.TimeLeft < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.TimeLeft));
    }

    if (input.Life < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.Life));
    }
  }
}
