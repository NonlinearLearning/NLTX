namespace Terraria.Projectile;

/// <summary>
/// 保存射弹定义、行为键及默认阵营和更新参数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：type（第 118 行）； aiStyle（第 136 行）； hostile（第 148 行）； friendly（第 154 行）； extraUpdates（第 196
/// 行）； noEnchantments（第 232 行）。
/// </para>
/// <para>重组说明：CatalogRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 245 行。</para>
/// </remarks>
public struct ProjectileDefinitionComponent
{
  public ProjectileDefinitionComponent(
    int type,
    bool friendly,
    bool hostile,
    int extraUpdates,
    int catalogRevision = 0)
    : this(
      type,
      behaviorKey: 0,
      friendly,
      hostile,
      extraUpdates,
      catalogRevision)
  {
  }

  public ProjectileDefinitionComponent(
    int projectileType,
    int behaviorKey,
    bool friendlyDefault,
    bool hostileDefault,
    int extraUpdates,
    int catalogRevision = 0,
    bool noEnchantments = false)
  {
    ProjectileType = projectileType;
    BehaviorKey = behaviorKey;
    FriendlyDefault = friendlyDefault;
    HostileDefault = hostileDefault;
    ExtraUpdates = extraUpdates;
    CatalogRevision = catalogRevision;
    NoEnchantments = noEnchantments;
  }

  public int ProjectileType;
  public int BehaviorKey;
  public bool FriendlyDefault;
  public bool HostileDefault;
  public int ExtraUpdates;
  public int CatalogRevision;
  public bool NoEnchantments;

  public int Type
  {
    get => ProjectileType;
    set => ProjectileType = value;
  }

  public bool Friendly
  {
    get => FriendlyDefault;
    set => FriendlyDefault = value;
  }

  public bool Hostile
  {
    get => HostileDefault;
    set => HostileDefault = value;
  }
}
