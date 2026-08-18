namespace Terraria.Dome.Simulation.Items.Components;

public struct ItemUseStateComponent
{
  public int CooldownTicks;
  public int AnimationTicks;
  public bool IsChanneling;

  public readonly bool CanUse => CooldownTicks <= 0;
}
