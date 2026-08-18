using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerLifecycleComponent
{
  public bool IsActive;
  public int RespawnTicks;
  public SimulationVector Spawn;
}
