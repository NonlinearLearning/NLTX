namespace Terraria.Dome.Simulation.Components;

public struct ProjectileBannerResponseComponent
{
  public ProjectileBannerResponseComponent(ushort bannerId)
  {
    BannerId = bannerId;
  }

  public ushort BannerId;
}
