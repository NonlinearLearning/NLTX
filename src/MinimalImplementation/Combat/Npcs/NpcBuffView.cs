using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Detached buff state for one active NPC generation.</summary>
public sealed record NpcBuffView
{
  public NpcBuffView(
    CombatEntityIdentity entity,
    short npcSlot,
    short npcType,
    IReadOnlyList<NpcBuffEntry> buffs,
    long npcRevision,
    long ownerRevision,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    Entity = entity;
    NpcSlot = npcSlot;
    NpcType = npcType;
    Buffs = Array.AsReadOnly(buffs.ToArray());
    NpcRevision = npcRevision;
    OwnerRevision = ownerRevision;
    CommitVersion = commitVersion;
  }

  public CombatEntityIdentity Entity { get; }

  public short NpcSlot { get; }

  public short NpcType { get; }

  public IReadOnlyList<NpcBuffEntry> Buffs { get; }

  public long NpcRevision { get; }

  public long OwnerRevision { get; }

  public CommitVersion CommitVersion { get; }
}
