namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestLifecycleComponent(bool IsActive, bool IsTombstoned)
{
  public static ChestLifecycleComponent Active => new(true, false);

  public static ChestLifecycleComponent Destroyed => new(false, true);
}
