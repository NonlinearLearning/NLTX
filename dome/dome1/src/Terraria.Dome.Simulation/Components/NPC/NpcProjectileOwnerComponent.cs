using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct NpcProjectileOwnerComponent(NpcHandle Owner)
{
  public NpcHandle OwnerHandle => Owner;

  public bool IsNpcProjectile => true;
}
