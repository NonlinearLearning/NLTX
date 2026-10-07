using System;

namespace Terraria.Projectile;

public readonly record struct ProjectileDefinitionHydrationContext
{
  public ProjectileDefinitionHydrationContext(
    int catalogRevision,
    int npcCapacity,
    int playerCapacity = 255)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(catalogRevision);
    ArgumentOutOfRangeException.ThrowIfNegative(npcCapacity);
    ArgumentOutOfRangeException.ThrowIfNegative(playerCapacity);
    CatalogRevision = catalogRevision;
    NpcCapacity = npcCapacity;
    PlayerCapacity = playerCapacity;
  }

  public int CatalogRevision { get; }

  public int NpcCapacity { get; }

  public int PlayerCapacity { get; }
}
