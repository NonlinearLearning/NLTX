namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TileEntityLinkInvalidationQuery
{
  public static TileEntityLinkComponent InvalidateIfMissing(
    TileEntityLinkComponent link,
    TileEntityStore store)
  {
    return link.IsLinked && !store.TryGet(link.LinkedEntityId, out _)
      ? TileEntityLinkComponent.None
      : link;
  }
}
