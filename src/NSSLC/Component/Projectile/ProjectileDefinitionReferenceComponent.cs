using System;

namespace Terraria.Projectile;

public readonly record struct ProjectileDefinitionReferenceComponent
{
  public ProjectileDefinitionReferenceComponent(
    int projectileType,
    int catalogRevision = 0)
    : this(projectileType, behaviorKey: 0, catalogRevision)
  {
  }

  public ProjectileDefinitionReferenceComponent(
    int projectileType,
    int behaviorKey,
    int catalogRevision)
  {
    if (projectileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }

    if (catalogRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(catalogRevision));
    }

    ProjectileType = projectileType;
    BehaviorKey = behaviorKey;
    CatalogRevision = catalogRevision;
  }

  public int ProjectileType { get; }

  public int BehaviorKey { get; }

  public int CatalogRevision { get; }

  public bool HasProjectileType => ProjectileType > 0;
}
