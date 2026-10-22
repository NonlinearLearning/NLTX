namespace MinimalImplementation.Combat.Npcs;

public sealed class NpcAiPlan {
  internal NpcAiPlan(
      NpcStatePlan state, NpcMotionState motion, bool encourageDespawn,
      NpcCombatStats? combatStats = null) {
    State = state;
    Motion = motion;
    EncourageDespawn = encourageDespawn;
    CombatStats = combatStats;
  }

  internal NpcStatePlan State { get; }
  internal NpcMotionState Motion { get; }
  internal NpcCombatStats? CombatStats { get; }

  /// <summary>AI commit caps the activity timer at ten; nearby players may refresh it.</summary>
  public bool EncourageDespawn { get; }
}
