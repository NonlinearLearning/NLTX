using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹生成来源、关联旗帜、掉落和召唤物来源信息。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：noDropItem（第 108 行）； miscText（第 222 行）； originatedFromActivableTile（第 240 行）；
/// bannerIdToRespondTo（第 260 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
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
