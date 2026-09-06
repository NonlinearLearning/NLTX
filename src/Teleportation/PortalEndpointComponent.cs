using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

public struct PortalEndpointComponent
{
  public float Angle;
  public int Direction;
  public int Form;
  public EntityReference PortalEntity;
  public TileCoordinate SupportTile;

  public EntityReference PortalEntityId
  {
    get => PortalEntity;
    set => PortalEntity = value;
  }

}
