namespace Terraria.Npc;

public sealed class NpcLifetimeComponent
{
  public NpcLifetimeComponent(
    int remainingTicks,
    float populationSlotCost,
    bool countsAgainstPopulation)
  {
    RemainingTicks = remainingTicks;
    PopulationSlotCost = populationSlotCost;
    CountsAgainstPopulation = countsAgainstPopulation;
  }

  public int RemainingTicks { get; set; }

  public float PopulationSlotCost { get; }

  public bool CountsAgainstPopulation { get; }

  public bool DespawnEncouraged { get; set; }

  public NpcDespawnReason PendingDespawnReason { get; set; }

  public bool IsExpired => RemainingTicks <= 0;

  public bool IsPendingDespawn => PendingDespawnReason != NpcDespawnReason.None;

  public float EffectivePopulationCost => CountsAgainstPopulation ? PopulationSlotCost : 0f;
}
