namespace Terraria.WorldSession.Events.Banners;

public sealed class BannerClaimNotificationStateComponent
{
  public BannerClaimNotificationStateComponent(bool hasNewClaimableBanners = false)
  {
    HasNewClaimableBanners = hasNewClaimableBanners;
  }

  public bool HasNewClaimableBanners { get; internal set; }
}
