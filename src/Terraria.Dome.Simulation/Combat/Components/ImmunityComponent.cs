namespace Terraria.Dome.Simulation.Combat.Components;

public struct ImmunityComponent
{
  public bool IsImmune => RemainingTicks > 0;

  public int RemainingTicks;
}
