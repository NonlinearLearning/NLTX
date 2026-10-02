namespace Terraria.SpatialMotionPhysics;

public static class MinecartTrackQuery
{
  public static MinecartTrackSample Read(
    MinecartTrackDefinitionCatalog catalog,
    int frame)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.Read(frame);
  }
}
