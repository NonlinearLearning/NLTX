namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileDamageComponent(int Amount, int HitCount = 0)
{
  public ProjectileDamageComponent RegisterHit()
  {
    if (HitCount == int.MaxValue)
    {
      return this;
    }

    return this with { HitCount = HitCount + 1 };
  }
}
