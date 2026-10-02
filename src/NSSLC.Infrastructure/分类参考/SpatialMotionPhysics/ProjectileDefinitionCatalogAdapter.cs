namespace Terraria.SpatialMotionPhysics;

public static class ProjectileDefinitionCatalogAdapter
{
  public static ProjectileDefinitionCatalog Create(int projectileCount)
  {
    return new ProjectileDefinitionCatalog(projectileCount);
  }
}
