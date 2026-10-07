using System.Numerics;

namespace Terraria.Player.Mount;

public static class MountSpecialVehicleCatalogQuery
{
  public static Vector2 GetScutlixEyePosition(
    MountSpecialVehicleCatalogDefinition catalog,
    int index)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.GetScutlixEyePosition(index);
  }
}
