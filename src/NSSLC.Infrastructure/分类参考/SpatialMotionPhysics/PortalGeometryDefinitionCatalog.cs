namespace Terraria.SpatialMotionPhysics;

public sealed class PortalGeometryDefinitionCatalog
{
  private PortalGeometryDefinitionCatalog(int portalsPerPerson)
  {
    PortalsPerPerson = portalsPerPerson;
  }

  public int PortalsPerPerson { get; }

  public static PortalGeometryDefinitionCatalog CreateDefault()
  {
    return new PortalGeometryDefinitionCatalog(2);
  }
}
