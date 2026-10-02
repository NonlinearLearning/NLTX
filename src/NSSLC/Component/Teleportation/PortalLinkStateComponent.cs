using Terraria.Relationships;

namespace Terraria.Teleportation;

public struct PortalLinkStateComponent
{
  public int PortalGroup;
  public EntityReference? PeerEndpoint;

  public bool IsPairComplete => PeerEndpoint.HasValue;
}
