using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Teleportation;

public struct PortalEndpointStateComponent
{
  public PortalEndpointStateComponent(PlayerHandle owner, byte pairIndex, int portalGroup)
  {
    Owner = owner;
    PairIndex = pairIndex;
    PortalGroup = portalGroup;
    Direction = 1;
    Active = true;
  }

  public PlayerHandle Owner;
  public byte PairIndex;
  public int PortalGroup;
  public float Angle;
  public int Form;
  public sbyte Direction;
  public bool SupportedByTile;
  public bool Active;

  public int PortalColorIndex => checked(Owner.Value * 2 + PairIndex);
}
