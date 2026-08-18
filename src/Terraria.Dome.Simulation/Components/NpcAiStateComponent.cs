using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Components;

public struct NpcAiStateComponent
{
  public NpcAiStateComponent(float chaseSpeed)
  {
    ChaseSpeed = chaseSpeed;
    BehaviorId = NpcBehaviorId.OrdinaryChase;
    PhaseTicks = 0;
  }

  public float ChaseSpeed;
  public NpcBehaviorId BehaviorId;
  public int PhaseTicks;
}
