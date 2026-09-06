using System;

namespace Terraria.Npc;

// status: proposed
public sealed class NpcPopulationLifetimeState
{
  public NpcPopulationLifetimeState(
    long remainingTicks = 0,
    float populationSlotCost = 0.0f,
    bool countsAgainstPopulation = false,
    bool despawnEncouraged = false,
    NpcDespawnReason pendingDespawnReason = NpcDespawnReason.None)
  {
    if (remainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    if (populationSlotCost < 0.0f
      || float.IsNaN(populationSlotCost)
      || float.IsInfinity(populationSlotCost))
    {
      throw new ArgumentOutOfRangeException(nameof(populationSlotCost));
    }

    if (!countsAgainstPopulation && populationSlotCost > 0.0f)
    {
      throw new InvalidOperationException(
        "An entity outside population counting cannot consume a slot cost.");
    }

    RemainingTicks = remainingTicks;
    PopulationSlotCost = populationSlotCost;
    CountsAgainstPopulation = countsAgainstPopulation;
    DespawnEncouraged = despawnEncouraged;
    PendingDespawnReason = pendingDespawnReason;
  }

  public long RemainingTicks { get; }

  public float PopulationSlotCost { get; }

  public bool CountsAgainstPopulation { get; }

  public bool DespawnEncouraged { get; }

  public NpcDespawnReason PendingDespawnReason { get; }

  public bool IsExpired => RemainingTicks == 0;

  public float EffectivePopulationCost =>
    CountsAgainstPopulation ? PopulationSlotCost : 0.0f;
}
