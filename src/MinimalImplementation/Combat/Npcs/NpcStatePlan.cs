namespace MinimalImplementation.Combat.Npcs;

/// <summary>Validated complete NPC state replacement awaiting owner-thread commit.</summary>
public sealed class NpcStatePlan
{
  internal NpcStatePlan(long managerRevision, Npc npc, NpcStateUpdate update)
  {
    ManagerRevision = managerRevision;
    Npc = npc;
    ExpectedNpcRevision = npc.Revision;
    Update = update;
  }

  internal long ManagerRevision { get; }

  internal Npc Npc { get; }

  internal long ExpectedNpcRevision { get; }

  internal NpcStateUpdate Update { get; }
}
