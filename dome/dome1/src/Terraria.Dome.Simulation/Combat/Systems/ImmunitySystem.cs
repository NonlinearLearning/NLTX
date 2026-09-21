using Terraria.Dome.Simulation.Combat.Components;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class ImmunitySystem
{
  public void Tick(ref ImmunityComponent immunity)
  {
    if (immunity.RemainingTicks > 0)
    {
      immunity.RemainingTicks--;
    }
  }
}
