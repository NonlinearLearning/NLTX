using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Detached player TagEffect state for one producer or owner-thread read.</summary>
public sealed class TagEffectSnapshotView
{
  public TagEffectSnapshotView(
    byte player,
    SessionEpoch sessionEpoch,
    short effectType,
    bool netSync,
    bool syncProcs,
    IReadOnlyList<int> npcTimes,
    IReadOnlyList<int> procTimes,
    IReadOnlyList<CombatEntityIdentity?> npcIdentities,
    long revision,
    CommitVersion commitVersion,
    bool isDirty)
  {
    ArgumentNullException.ThrowIfNull(npcTimes);
    ArgumentNullException.ThrowIfNull(procTimes);
    ArgumentNullException.ThrowIfNull(npcIdentities);
    if (!sessionEpoch.IsValid || !commitVersion.IsValid || revision <= 0 ||
        npcTimes.Count != procTimes.Count || npcTimes.Count != npcIdentities.Count)
    {
      throw new ArgumentException("TagEffect snapshot dimensions and versions must be valid.");
    }

    Player = player;
    SessionEpoch = sessionEpoch;
    EffectType = effectType;
    NetSync = netSync;
    SyncProcs = syncProcs;
    NpcTimes = Array.AsReadOnly(npcTimes.ToArray());
    ProcTimes = Array.AsReadOnly(procTimes.ToArray());
    NpcIdentities = Array.AsReadOnly(npcIdentities.ToArray());
    Revision = revision;
    CommitVersion = commitVersion;
    IsDirty = isDirty;
  }

  public byte Player { get; }

  public SessionEpoch SessionEpoch { get; }

  public short EffectType { get; }

  public bool NetSync { get; }

  public bool SyncProcs { get; }

  public IReadOnlyList<int> NpcTimes { get; }

  public IReadOnlyList<int> ProcTimes { get; }

  public IReadOnlyList<CombatEntityIdentity?> NpcIdentities { get; }

  public long Revision { get; }

  public CommitVersion CommitVersion { get; }

  public bool IsDirty { get; }
}
