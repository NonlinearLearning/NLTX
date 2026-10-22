using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>G2-owned world facts for one type-3 Zombie update.</summary>
public sealed class NpcZombieTickInput {
  public NpcZombieTickInput(
      CombatEntityIdentity entity,
      long sourceNpcRevision,
      ulong sourceWorldEpoch,
      CommitVersion sourceWorldCommitVersion,
      NpcZombieEnvironment environment,
      bool confused) {
    ArgumentNullException.ThrowIfNull(environment);
    if (!entity.IsValid) {
      throw new ArgumentException(
          "Zombie input requires a valid entity generation.", nameof(entity));
    }
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceNpcRevision);
    if (sourceWorldEpoch == 0 || !sourceWorldCommitVersion.IsValid) {
      throw new ArgumentException("Zombie facts require a valid source world revision.");
    }
    Entity = entity;
    SourceNpcRevision = sourceNpcRevision;
    SourceWorldEpoch = sourceWorldEpoch;
    SourceWorldCommitVersion = sourceWorldCommitVersion;
    Environment = environment;
    Confused = confused;
  }

  public CombatEntityIdentity Entity { get; }

  public long SourceNpcRevision { get; }

  public ulong SourceWorldEpoch { get; }

  public CommitVersion SourceWorldCommitVersion { get; }

  public NpcZombieEnvironment Environment { get; }

  public bool Confused { get; }
}
