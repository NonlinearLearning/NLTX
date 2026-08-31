namespace Terraria.Dome.Simulation.Components;

public struct ProjectileUpdateCountComponent
{
  public int Count;

  public void Advance()
  {
    if (Count < int.MaxValue)
    {
      Count++;
    }
  }
}
