using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public static class MinecartPresentationQuery
{
  public static Vector2 GetTexturePosition(
    MinecartPresentationDefinitionCatalog catalog,
    int frame)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.GetTexturePosition(frame);
  }

  public static int GetTileHeight(
    MinecartPresentationDefinitionCatalog catalog,
    int frame)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.GetTileHeight(frame);
  }
}
