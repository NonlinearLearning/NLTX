using System;
using System.Numerics;

namespace Terraria.Projectile;

public readonly record struct ProjectileSpawnCommand
{
  public ProjectileSpawnCommand(
    int projectileType,
    ProjectileOwnerReference owner,
    Vector2 center,
    Vector2 velocity,
    int damage,
    int originalDamage,
    float knockback,
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    int bannerIdToRespondTo = 0)
  {
    if (projectileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }

    ValidateFinite(center, nameof(center));
    ValidateFinite(velocity, nameof(velocity));
    ValidateFinite(knockback, nameof(knockback));
    ValidateFinite(ai0, nameof(ai0));
    ValidateFinite(ai1, nameof(ai1));
    ValidateFinite(ai2, nameof(ai2));

    if ((uint)bannerIdToRespondTo > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(bannerIdToRespondTo));
    }

    ProjectileType = projectileType;
    Owner = owner;
    Center = center;
    Velocity = velocity;
    Damage = damage;
    OriginalDamage = originalDamage;
    Knockback = knockback;
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    BannerIdToRespondTo = bannerIdToRespondTo;
  }

  public int ProjectileType { get; }

  public ProjectileOwnerReference Owner { get; }

  public Vector2 Center { get; }

  public Vector2 Velocity { get; }

  public int Damage { get; }

  public int OriginalDamage { get; }

  public float Knockback { get; }

  public float Ai0 { get; }

  public float Ai1 { get; }

  public float Ai2 { get; }

  public int BannerIdToRespondTo { get; }

  private static void ValidateFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
