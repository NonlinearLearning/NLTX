namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TileEntityLinkComponent(int LinkedEntityId)
{
  public bool IsLinked => LinkedEntityId > 0;

  public static TileEntityLinkComponent None => new(0);

  public TileEntityLinkComponent Invalidate()
  {
    return None;
  }
}
