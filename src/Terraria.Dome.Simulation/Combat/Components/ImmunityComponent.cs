namespace Terraria.Dome.Simulation.Combat.Components;

public struct ImmunityComponent
{
  public const int StandardShadowDodgeTicks = 80;

  public bool IsImmune => RemainingTicks > 0;

  public int RemainingTicks;

  public void ApplyShadowDodge()
  {
    RemainingTicks = StandardShadowDodgeTicks;
  }
}
