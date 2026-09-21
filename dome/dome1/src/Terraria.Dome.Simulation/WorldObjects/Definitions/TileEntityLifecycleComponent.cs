namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TileEntityLifecycleComponent(bool RequiresUpdates, bool IsActive)
{
  public static TileEntityLifecycleComponent Inactive => new(false, false);

  public TileEntityLifecycleComponent Activate(bool requiresUpdates)
  {
    return new(requiresUpdates, true);
  }

  public TileEntityLifecycleComponent Deactivate()
  {
    return Inactive;
  }
}
