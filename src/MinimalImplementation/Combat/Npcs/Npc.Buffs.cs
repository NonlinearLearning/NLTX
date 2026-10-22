using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed partial class Npc
{
  private IReadOnlyList<NpcBuffEntry> _buffs = Array.Empty<NpcBuffEntry>();

  internal IReadOnlyList<NpcBuffEntry> Buffs => _buffs;

  internal NpcBuffView CaptureBuffs(long ownerRevision)
  {
    return new NpcBuffView(Entity, Slot, Type, _buffs, Revision, ownerRevision, CommitVersion);
  }

  internal void ApplyBuffs(IReadOnlyList<NpcBuffEntry> buffs, CommitVersion commitVersion)
  {
    _buffs = Array.AsReadOnly(buffs.ToArray());
    Revision++;
    CommitVersion = commitVersion;
  }

  private void ClearBuffs()
  {
    _buffs = Array.Empty<NpcBuffEntry>();
  }
}
