using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Committed death outbox entry; retained until P04 and P07 have settled it.</summary>
public sealed class NpcDeathRecord {
  internal NpcDeathRecord(
      NpcView npc, CommitVersion sourceCommitVersion, IReadOnlyList<SessionEpoch> interactions,
      SessionEpoch? lastInteraction, IReadOnlyList<NpcDropIntent> drops) {
    Npc = npc;
    SourceCommitVersion = sourceCommitVersion;
    Interactions = Array.AsReadOnly(interactions.ToArray());
    LastInteraction = lastInteraction;
    Drops = Array.AsReadOnly(drops.ToArray());
    KillCredit = interactions.Count > 0 ? NpcKillCredit.ForCoreType(npc.Type) : null;
  }

  public NpcView Npc { get; }
  public CommitVersion SourceCommitVersion { get; }
  public IReadOnlyList<SessionEpoch> Interactions { get; }
  public SessionEpoch? LastInteraction { get; }
  public IReadOnlyList<NpcDropIntent> Drops { get; }
  public NpcKillCredit? KillCredit { get; }

  // All currently migrated types (1/2/3) pass IsNPCValidForBestiaryKillCredit.
  // This credit precedes statue item-loot suppression in NPC.NPCLoot.
  public bool ContributesKill => KillCredit is not null;
}
