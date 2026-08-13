namespace Terraria.Dome.Simulation.Components;

public struct NpcAiStateComponent
{
  public NpcAiStateComponent(float chaseSpeed)
  {
    ChaseSpeed = chaseSpeed;
  }

  public float ChaseSpeed;
}
