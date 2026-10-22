namespace MinimalImplementation.Combat.Npcs;

/// <summary>Retirement and already-rolled loot; retry this data without rolling again.</summary>
public sealed class NpcDeathPlan {
  internal NpcDeathPlan(NpcRetirementPlan retirement, IReadOnlyList<NpcDropIntent> drops) {
    Retirement = retirement;
    Drops = Array.AsReadOnly(drops.ToArray());
  }

  internal NpcRetirementPlan Retirement { get; }
  public IReadOnlyList<NpcDropIntent> Drops { get; }
}
