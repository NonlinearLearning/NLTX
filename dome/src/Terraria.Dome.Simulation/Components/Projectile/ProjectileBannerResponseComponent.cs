namespace Terraria.Dome.Simulation.Components;

public struct ProjectileBannerResponseComponent
{
  public ProjectileBannerResponseComponent(ushort bannerId)
  {
    BannerId = bannerId;
  }

  public ushort BannerId;

  public ushort BannerIdToRespondTo
  {
    get => BannerId;
    set => BannerId = value;
  }

  public bool HasBannerResponse => BannerId != 0;
}
