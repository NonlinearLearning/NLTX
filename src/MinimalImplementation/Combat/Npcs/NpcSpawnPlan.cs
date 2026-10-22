namespace MinimalImplementation.Combat.Npcs;

/// <summary>Owner-thread spawn intent held between preparation and commit.</summary>
public sealed class NpcSpawnPlan
{
  internal NpcSpawnPlan(
    long managerRevision,
    NpcSpawnRequest request,
    NpcDefinition definition,
    short slot)
  {
    ManagerRevision = managerRevision;
    Request = request;
    Definition = definition;
    Slot = slot;
  }

  internal long ManagerRevision { get; }

  internal NpcSpawnRequest Request { get; }

  internal NpcDefinition Definition { get; }

  internal short Slot { get; }
}
