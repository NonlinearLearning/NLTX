using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckActiveTimerRefreshPolicy
{
  public const int Version1456ActiveTime = 750;

  public static NpcCheckActiveTimerRefreshDecision Evaluate(
    NpcCheckActiveTimerRefreshInput input)
  {
    Validate(input);
    if (!input.IsNpcActive || input.DoesNotDespawnToInactivityAndCountsNpcSlots ||
        !input.HasScreenRangePlayer)
    {
      return new(false, input.TimeLeft, input.DespawnEncouraged);
    }

    return new(true, input.ActiveTime, false);
  }

  private static void Validate(NpcCheckActiveTimerRefreshInput input)
  {
    if (input.ActiveTime <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.ActiveTime));
    }
  }
}
