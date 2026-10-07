using System;

namespace Terraria.Projectile;

/// <summary>
/// World-level absolute expiry ticks for per-projectile-type NPC immunity.
/// </summary>
public sealed class ProjectileStaticNpcImmunityRegistryComponent
{
  private readonly uint[][] _immunityExpiryByProjectileType;

  public ProjectileStaticNpcImmunityRegistryComponent(
    int projectileTypeCapacity,
    int npcCapacity)
  {
    if (projectileTypeCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileTypeCapacity));
    }

    if (npcCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcCapacity));
    }

    _immunityExpiryByProjectileType = new uint[projectileTypeCapacity][];
    for (int projectileType = 0;
      projectileType < _immunityExpiryByProjectileType.Length;
      projectileType++)
    {
      _immunityExpiryByProjectileType[projectileType] = new uint[npcCapacity];
    }
  }

  internal int NpcCapacity => _immunityExpiryByProjectileType[0].Length;

  internal uint[][] ImmunityExpiryByProjectileType =>
    _immunityExpiryByProjectileType;
}
