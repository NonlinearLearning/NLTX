namespace Terraria.Dome.Simulation.Components;

public struct ProjectileReflectionComponent
{
  public bool HasReflected { get; set; }

  public bool Reflected
  {
    get => HasReflected;
    set => HasReflected = value;
  }

  public bool HasBeenReflected => HasReflected;
}
