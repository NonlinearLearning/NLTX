namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileMiscTextComponent(string Value)
{
  public string MiscText => Value;
}
