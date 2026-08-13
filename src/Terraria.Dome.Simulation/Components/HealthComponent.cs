namespace Terraria.Dome.Simulation.Components;

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
