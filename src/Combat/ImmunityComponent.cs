namespace Terraria.Combat;

public struct ImmunityComponent
{
  public ImmunityComponent(int remainingTicks)
  {
    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks;
}
