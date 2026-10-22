using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class NpcManager
{
  internal IReadOnlyList<NpcBuffView> CaptureAllBuffs()
  {
    if (!IsWorldScopeCurrent())
    {
      return Array.Empty<NpcBuffView>();
    }

    var result = new List<NpcBuffView>(_byIdentity.Count);
    foreach (Npc? npc in _slots)
    {
      if (npc is { IsActive: true } && _identityAuthority.IsActive(npc.Entity))
      {
        result.Add(npc.CaptureBuffs(_revision));
      }
    }

    return result.AsReadOnly();
  }

  internal bool TryCaptureBuffs(
    short slot,
    CombatEntityIdentity expectedIdentity,
    out NpcBuffView? snapshot)
  {
    if (IsWorldScopeCurrent() && slot >= 0 && slot < _slots.Length &&
        _slots[slot] is Npc npc && npc.IsActive &&
        npc.Entity == expectedIdentity && _identityAuthority.IsActive(expectedIdentity) &&
        _byIdentity.TryGetValue(expectedIdentity, out Npc? current) && current == npc)
    {
      snapshot = npc.CaptureBuffs(_revision);
      return true;
    }

    snapshot = null;
    return false;
  }

  internal (OwnerCommitResult Result, NpcBuffView? Snapshot) CommitBuffs(
    NpcBuffView expected,
    IReadOnlyList<NpcBuffEntry> buffs)
  {
    ArgumentNullException.ThrowIfNull(expected);
    ArgumentNullException.ThrowIfNull(buffs);
    if (!IsWorldScopeCurrent() || expected.NpcSlot < 0 || expected.NpcSlot >= _slots.Length ||
        _slots[expected.NpcSlot] is not Npc npc || !npc.IsActive ||
        npc.Entity != expected.Entity || npc.Type != expected.NpcType ||
        npc.Revision != expected.NpcRevision || _revision != expected.OwnerRevision ||
        npc.CommitVersion != expected.CommitVersion ||
        !_byIdentity.TryGetValue(expected.Entity, out Npc? current) || current != npc ||
        !_identityAuthority.IsActive(expected.Entity))
    {
      return (new OwnerCommitResult(
        OwnerCommitStatus.Stale,
        null,
        ProtocolErrorCode.StaleEntity,
        "NPC generation or buff state changed after buff preparation."), null);
    }

    if (npc.Revision == long.MaxValue || _revision == long.MaxValue)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC revision range is exhausted."), null);
    }

    CommitVersion nextVersion;
    try
    {
      nextVersion = _commitVersion.Next();
    }
    catch (InvalidOperationException)
    {
      return (OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "NPC commit version range is exhausted."), null);
    }

    npc.ApplyBuffs(buffs, nextVersion);
    CommitMutation(nextVersion);
    return (OwnerCommitResult.Applied(nextVersion), npc.CaptureBuffs(_revision));
  }
}
