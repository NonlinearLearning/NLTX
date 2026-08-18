using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerRespawnSystem
{
  public bool TryRespawn(
    ref PlayerLifecycleComponent lifecycle,
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref HealthComponent health,
    SimulationVector spawn)
  {
    if (lifecycle.IsActive || lifecycle.RespawnTicks > 0)
    {
      return false;
    }

    transform.X = spawn.X;
    transform.Y = spawn.Y;
    velocity.X = 0.0f;
    velocity.Y = 0.0f;
    health.Current = health.Maximum;
    lifecycle.IsActive = true;
    lifecycle.RespawnTicks = 0;
    return true;
  }
}
