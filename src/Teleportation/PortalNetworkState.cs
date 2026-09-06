using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

public sealed class PortalNetworkState
{
  public EntityReference? FirstPortal;
  public TileCoordinate? FirstPortalSupport;
  public bool IsComplete => FirstPortal.HasValue && SecondPortal.HasValue;
  public int LastPortalColorIndex = -1;
  public EntityReference? SecondPortal;
  public TileCoordinate? SecondPortalSupport;
  public long? UpdatedAtTick;
}
