using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager {
  /// <summary>
  /// Captures every occupied active slot or returns an explicit failure. Unlike
  /// Capture(), an invalid world scope cannot become a successful empty population.
  /// </summary>
  public NpcPreparation<NpcPopulationSnapshot> CapturePopulation() {
    if (!IsWorldScopeCurrent()) {
      return NpcPreparation<NpcPopulationSnapshot>.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC population belongs to an invalidated world scope.");
    }
    long revision = _revision;
    CommitVersion version = _commitVersion;
    var entries = new List<NpcPopulationEntry>(_byIdentity.Count);
    for (int slot = 0; slot < _slots.Length; slot++) {
      Npc? npc = _slots[slot];
      if (npc is null) {
        continue;
      }
      if (!npc.IsActive || npc.Slot != slot || npc.Entity.WorldScope != _worldScope ||
          !_byIdentity.TryGetValue(npc.Entity, out Npc? indexed) || indexed != npc ||
          !_identityAuthority.IsActive(npc.Entity)) {
        return NpcPreparation<NpcPopulationSnapshot>.Rejected(
            ProtocolErrorCode.StaleEntity, "An occupied NPC slot has no current active identity.");
      }
      entries.Add(new NpcPopulationEntry(npc.Capture(), npc.Home));
    }
    var snapshot = new NpcPopulationSnapshot(
        this, _worldScope, _slots.Length, revision, version, entries);
    OwnerCommitResult validation = Validate(snapshot);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return NpcPreparation<NpcPopulationSnapshot>.Rejected(
          validation.ErrorCode ?? ProtocolErrorCode.Conflict,
          validation.Detail ?? "NPC population changed during capture.");
    }
    return NpcPreparation<NpcPopulationSnapshot>.Prepared(snapshot);
  }

  /// <summary>
  /// Rechecks scope, owner revision and every identity, including empty captures.
  /// </summary>
  public OwnerCommitResult Validate(NpcPopulationSnapshot snapshot) {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!ReferenceEquals(snapshot.Owner, this) || !IsWorldScopeCurrent() ||
        snapshot.WorldScope != _worldScope) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC population has a different owner or world scope.");
    }
    if (snapshot.ManagerRevision != _revision || snapshot.CommitVersion != _commitVersion ||
        snapshot.SlotCapacity != _slots.Length || snapshot.Entries.Count != _byIdentity.Count) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "NPC population changed after capture.");
    }
    foreach (NpcPopulationEntry entry in snapshot.Entries) {
      NpcView view = entry.Npc;
      if (view.Slot < 0 || view.Slot >= _slots.Length ||
          _slots[view.Slot] is not Npc npc || !npc.IsActive || npc.Entity != view.Entity ||
          npc.Revision != view.Revision || npc.CommitVersion != view.CommitVersion ||
          npc.Home != entry.Home ||
          !_byIdentity.TryGetValue(view.Entity, out Npc? indexed) || indexed != npc ||
          !_identityAuthority.IsActive(view.Entity)) {
        return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity, "A captured NPC generation changed or expired.");
      }
    }
    if (!IsWorldScopeCurrent() || snapshot.ManagerRevision != _revision ||
        snapshot.CommitVersion != _commitVersion) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "NPC population changed during validation.");
    }
    return OwnerCommitResult.Applied(_commitVersion);
  }

  /// <summary>
  /// Housing producers supply authoritative tiles, not client packet 60 values.
  /// World bounds, housing rules and packet 60 publication remain at their own boundaries.
  /// </summary>
  public NpcPreparation<NpcHomePlan> PrepareHomeUpdate(
      CombatEntityIdentity identity, long expectedRevision, NpcHomeState home) {
    ArgumentNullException.ThrowIfNull(home);
    if (!TryGet(identity, out NpcView? view) || view is null ||
        view.Revision != expectedRevision) {
      return NpcPreparation<NpcHomePlan>.Rejected(
          ProtocolErrorCode.StaleEntity, "Housing update requires the current NPC generation.");
    }
    if (!home.HasValidCoordinates) {
      return NpcPreparation<NpcHomePlan>.Rejected(
          ProtocolErrorCode.InvalidLength, "NPC home tiles cannot be below the unset sentinel.");
    }
    return NpcPreparation<NpcHomePlan>.Prepared(
        new NpcHomePlan(_revision, _byIdentity[identity], home));
  }

  public OwnerCommitResult Validate(NpcHomePlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    Npc npc = plan.Npc;
    if (!IsWorldScopeCurrent() || plan.ManagerRevision != _revision || !npc.IsActive ||
        npc.Revision != plan.ExpectedNpcRevision || npc.Slot < 0 ||
        npc.Slot >= _slots.Length || _slots[npc.Slot] != npc ||
        !_byIdentity.TryGetValue(npc.Entity, out Npc? indexed) || indexed != npc ||
        !_identityAuthority.IsActive(npc.Entity)) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "NPC housing preparation changed or expired.");
    }
    if (_revision == long.MaxValue || npc.Revision == long.MaxValue ||
        _commitVersion.Value == long.MaxValue) {
      return OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady, "NPC housing commit version range is exhausted.");
    }
    return OwnerCommitResult.Applied(_commitVersion);
  }

  public (OwnerCommitResult Result, NpcPopulationEntry? Npc) Commit(NpcHomePlan plan) {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult validation = Validate(plan);
    if (validation.Status != OwnerCommitStatus.Applied) {
      return (validation, null);
    }
    CommitVersion version = _commitVersion.Next();
    plan.Npc.ApplyHome(plan.Home, version);
    CommitMutation(version);
    return (OwnerCommitResult.Applied(version),
        new NpcPopulationEntry(plan.Npc.Capture(), plan.Npc.Home));
  }
}
