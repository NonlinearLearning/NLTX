using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcChaseState(float Speed, float StoppingDistance);

public readonly record struct NpcTownHomeState(
  SimulationVector HomePosition,
  bool IsHomeless,
  int ReturnTimeoutTicks);

public struct NpcBehaviorStateComponent
{
  public NpcBehaviorStateComponent(
    NpcBehaviorId behaviorId,
    NpcChaseState chase,
    NpcTownHomeState townHome)
  {
    BehaviorId = behaviorId;
    Chase = chase;
    TownHome = townHome;
    PhaseTicks = 0;
  }

  public NpcBehaviorId BehaviorId;
  public NpcChaseState Chase;
  public NpcTownHomeState TownHome;
  public int PhaseTicks;
}
