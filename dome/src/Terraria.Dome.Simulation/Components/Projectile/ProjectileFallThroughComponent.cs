namespace Terraria.Dome.Simulation.Components;

public struct ProjectileFallThroughComponent
{
  public bool ShouldFallThrough;

  public bool FallThrough
  {
    get => ShouldFallThrough;
    set => ShouldFallThrough = value;
  }

  public bool ShouldFall => ShouldFallThrough;
}
