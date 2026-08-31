namespace Terraria.Dome.Simulation.Components;

public struct ProjectileDirectionComponent
{
  public ProjectileDirectionComponent(int horizontal)
  {
    Horizontal = horizontal < 0 ? -1 : 1;
  }

  public int Horizontal;
}
