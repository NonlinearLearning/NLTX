namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignLifecycleComponent(bool IsActive, bool IsTombstoned)
{
  public static SignLifecycleComponent Active => new(true, false);

  public static SignLifecycleComponent Deleted => new(false, true);
}
