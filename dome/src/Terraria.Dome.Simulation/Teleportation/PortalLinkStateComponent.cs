using Arch.Core;

namespace Terraria.Dome.Simulation.Teleportation;

public struct PortalLinkStateComponent
{
  public int PortalGroup;
  public Entity? PeerEndpoint;

  public bool IsPairComplete => PeerEndpoint.HasValue;
}
