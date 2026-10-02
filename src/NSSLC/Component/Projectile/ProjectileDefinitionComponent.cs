namespace Terraria.Projectile;

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
