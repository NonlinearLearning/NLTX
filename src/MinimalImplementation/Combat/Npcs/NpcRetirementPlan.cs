namespace MinimalImplementation.Combat.Npcs;

/// <summary>Validated death retirement held between preparation and owner-thread commit.</summary>
public sealed class NpcRetirementPlan
{
  internal NpcRetirementPlan(long managerRevision, Npc npc)
  {
    ManagerRevision = managerRevision;
    Npc = npc;
    ExpectedNpcRevision = npc.Revision;
  }

  internal long ManagerRevision { get; }

  internal Npc Npc { get; }

  internal long ExpectedNpcRevision { get; }
}
