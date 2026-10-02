using System;

namespace Terraria.Projectile;

public struct ProjectileSourceMetadataComponent
{
  public ProjectileSourceMetadataComponent()
  {
    BannerIdToRespondTo = 0;
    MiscText = string.Empty;
    OriginatedFromActivableTile = false;
    NoDropItem = false;
    IsNpcProjectile = false;
    MinionSpawnItemType = 0;
    MinionSpawnItemPrefix = 0;
  }

  public ProjectileSourceMetadataComponent(
    int bannerIdToRespondTo = 0,
    string miscText = "",
    bool originatedFromActivableTile = false,
    bool noDropItem = false,
    bool isNpcProjectile = false,
    ushort minionSpawnItemType = 0,
    int minionSpawnItemPrefix = 0)
  {
    ArgumentNullException.ThrowIfNull(miscText);
    if (minionSpawnItemPrefix < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(minionSpawnItemPrefix));
    }

    BannerIdToRespondTo = bannerIdToRespondTo;
    MiscText = miscText;
    OriginatedFromActivableTile = originatedFromActivableTile;
    NoDropItem = noDropItem;
    IsNpcProjectile = isNpcProjectile;
    MinionSpawnItemType = minionSpawnItemType;
    MinionSpawnItemPrefix = minionSpawnItemPrefix;
  }

  public int BannerIdToRespondTo;
  public string MiscText;
  public bool OriginatedFromActivableTile;
  public bool NoDropItem;
  public bool IsNpcProjectile;
  public ushort MinionSpawnItemType;
  public int MinionSpawnItemPrefix;
}
