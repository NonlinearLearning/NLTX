namespace Terraria.Dome.Simulation.Components;
//这应该是血量
public struct HealthComponent
{
  public HealthComponent(int current, int maximum)
  {
    Current = current;
    Maximum = maximum;
  }

  public int Current;
  public int Maximum;
}
