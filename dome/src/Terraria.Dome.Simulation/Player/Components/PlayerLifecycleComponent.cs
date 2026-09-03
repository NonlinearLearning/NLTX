using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerLifecycleComponent
{
  public bool IsActive;
  public bool IsDead;
  public int DeadTime;
  public int RespawnTicks;
  public SimulationVector Spawn;
}
