namespace MinimalImplementation.Combat.Npcs;

/// <summary>A trusted housing producer's change awaiting the NPC owner commit.</summary>
public sealed class NpcHomePlan {
  internal NpcHomePlan(long managerRevision, Npc npc, NpcHomeState home) {
    ManagerRevision = managerRevision;
    Npc = npc;
    ExpectedNpcRevision = npc.Revision;
    Home = home;
  }

  public NpcHomeState Home { get; }

  internal long ManagerRevision { get; }

  internal Npc Npc { get; }

  internal long ExpectedNpcRevision { get; }
}
