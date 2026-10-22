using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>G2-owned world facts needed by one type-2 eye update.</summary>
public sealed class NpcEyeTickInput {
  public NpcEyeTickInput(
      CombatEntityIdentity entity,
      long sourceNpcRevision,
      ulong sourceWorldEpoch,
      CommitVersion sourceWorldCommitVersion,
      bool isDay,
      double worldSurfaceTiles,
      bool confused) {
    if (!entity.IsValid) {
      throw new ArgumentException("Eye input requires a valid entity generation.", nameof(entity));
    }
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceNpcRevision);
    if (sourceWorldEpoch == 0 || !sourceWorldCommitVersion.IsValid) {
      throw new ArgumentException("Eye facts require a valid source world revision.");
    }
    Entity = entity;
    SourceNpcRevision = sourceNpcRevision;
    SourceWorldEpoch = sourceWorldEpoch;
    SourceWorldCommitVersion = sourceWorldCommitVersion;
    IsDay = isDay;
    WorldSurfaceTiles = worldSurfaceTiles;
    Confused = confused;
  }

  public CombatEntityIdentity Entity { get; }

  public long SourceNpcRevision { get; }

  public ulong SourceWorldEpoch { get; }

  public CommitVersion SourceWorldCommitVersion { get; }

  public bool IsDay { get; }

  public double WorldSurfaceTiles { get; }

  public bool Confused { get; }
}
