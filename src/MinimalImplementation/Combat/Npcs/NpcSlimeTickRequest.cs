using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

public sealed class NpcSlimeTickRequest {
  public NpcSlimeTickRequest(
      CombatEntityIdentity entity, long expectedRevision, float value,
      NpcSlimeEnvironment environment, bool confused, IReadOnlyList<NpcTargetPlayer> players) {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(players);
    Entity = entity;
    ExpectedRevision = expectedRevision;
    Value = value;
    Environment = environment;
    Confused = confused;
    Players = Array.AsReadOnly(players.ToArray());
  }

  public CombatEntityIdentity Entity { get; }
  public long ExpectedRevision { get; }

  /// <summary>Trusted NPC value at ExpectedRevision, including spawn-source overrides.</summary>
  public float Value { get; }

  public NpcSlimeEnvironment Environment { get; }
  public bool Confused { get; }
  public IReadOnlyList<NpcTargetPlayer> Players { get; }
}
