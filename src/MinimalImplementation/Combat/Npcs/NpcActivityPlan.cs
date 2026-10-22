namespace MinimalImplementation.Combat.Npcs;

/// <summary>One CheckActive result, prepared after this tick's AI and movement.</summary>
public sealed class NpcActivityPlan {
  internal NpcActivityPlan(
      NpcStatePlan state, int timeLeft, bool despawn, IReadOnlyList<byte> nearbyPlayerSlots) {
    State = state;
    TimeLeft = timeLeft;
    Despawn = despawn;
    NearbyPlayerSlots = Array.AsReadOnly(nearbyPlayerSlots.ToArray());
  }

  internal NpcStatePlan State { get; }
  public int TimeLeft { get; }
  public bool Despawn { get; }

  /// <summary>Core NPC slot contributions; apply to P02 only after successful commit.</summary>
  public IReadOnlyList<byte> NearbyPlayerSlots { get; }
}
