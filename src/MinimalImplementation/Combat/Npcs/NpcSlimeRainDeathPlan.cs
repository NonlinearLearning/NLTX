using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// A prepared slime-rain death event. The counter and resulting spawn remain with the
/// event owner; Combat only proves that the event belongs to a committed death generation.
/// </summary>
public sealed class NpcSlimeRainDeathPlan
{
  internal NpcSlimeRainDeathPlan(
      long managerRevision,
      NpcDeathRecord death,
      NpcSlimeRainDeathFacts facts,
      NpcSlimeRainDeathResult result)
  {
    ManagerRevision = managerRevision;
    Death = death;
    Facts = facts;
    Result = result;
  }

  internal long ManagerRevision { get; }

  internal NpcDeathRecord Death { get; }

  internal NpcSlimeRainDeathFacts Facts { get; }

  public NpcSlimeRainDeathResult Result { get; }
}
