using System.Numerics;
using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Prepared packet-92 money-ping increment bound to one NPC generation.</summary>
public sealed class NpcExtraValuePlan
{
  internal NpcExtraValuePlan(
      long managerRevision,
      Npc npc,
      long expectedNpcRevision,
      int nextExtraValue,
      Vector2 position)
  {
    ManagerRevision = managerRevision;
    Npc = npc;
    ExpectedNpcRevision = expectedNpcRevision;
    NextExtraValue = nextExtraValue;
    Position = position;
  }

  internal long ManagerRevision { get; }

  internal Npc Npc { get; }

  internal long ExpectedNpcRevision { get; }

  internal int NextExtraValue { get; }

  internal Vector2 Position { get; }
}
