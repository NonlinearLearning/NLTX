using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>G2-owned facts for one revision of a type-1 slime.</summary>
public sealed class NpcSlimeTickInput {
  public NpcSlimeTickInput(
      CombatEntityIdentity entity,
      long sourceNpcRevision,
      ulong sourceWorldEpoch,
      CommitVersion sourceWorldCommitVersion,
      float value,
      NpcSlimeEnvironment environment,
      bool confused) {
    ArgumentNullException.ThrowIfNull(environment);
    if (!entity.IsValid) {
      throw new ArgumentException(
          "Slime input requires a valid entity generation.", nameof(entity));
    }
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceNpcRevision);
    if (sourceWorldEpoch == 0 || !sourceWorldCommitVersion.IsValid) {
      throw new ArgumentException("Slime facts require a valid source world revision.");
    }
    Entity = entity;
    SourceNpcRevision = sourceNpcRevision;
    SourceWorldEpoch = sourceWorldEpoch;
    SourceWorldCommitVersion = sourceWorldCommitVersion;
    Value = value;
    Environment = environment;
    Confused = confused;
  }

  public CombatEntityIdentity Entity { get; }

  /// <summary>Revision that the G2-captured value and environment facts belong to.</summary>
  public long SourceNpcRevision { get; }

  public ulong SourceWorldEpoch { get; }

  public CommitVersion SourceWorldCommitVersion { get; }

  public float Value { get; }

  public NpcSlimeEnvironment Environment { get; }

  public bool Confused { get; }
}
