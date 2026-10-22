using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

public sealed class NpcZombieTickRequest {
  public NpcZombieTickRequest(
      CombatEntityIdentity entity, long expectedRevision, NpcZombieEnvironment environment,
      bool confused, IReadOnlyList<NpcTargetPlayer> players) {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(players);
    Entity = entity;
    ExpectedRevision = expectedRevision;
    Environment = environment;
    Confused = confused;
    Players = Array.AsReadOnly(players.ToArray());
  }

  public CombatEntityIdentity Entity { get; }
  public long ExpectedRevision { get; }
  public NpcZombieEnvironment Environment { get; }
  public bool Confused { get; }
  public IReadOnlyList<NpcTargetPlayer> Players { get; }
}
