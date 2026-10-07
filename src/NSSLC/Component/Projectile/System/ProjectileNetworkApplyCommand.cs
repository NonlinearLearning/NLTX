using System;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// Decoded packet-27 projectile state. The owner/identity pair is remote
/// protocol state and must not be replaced with the receiving slot index.
/// </summary>
public readonly record struct ProjectileNetworkApplyCommand
{
  public ProjectileNetworkApplyCommand(
    int ownerSlot,
    int identity,
    int projectileType,
    Vector2 position,
    Vector2 velocity,
    int damage,
    int originalDamage,
    float knockback,
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    int projectileUuid = -1,
    int bannerIdToRespondTo = 0)
  {
    if ((uint)ownerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerSlot));
    }

    if (identity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    if (projectileType <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }

    if (projectileUuid < -1 || projectileUuid >= 1000)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileUuid));
    }

    if ((uint)bannerIdToRespondTo > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(bannerIdToRespondTo));
    }

    ValidateFinite(position, nameof(position));
    ValidateFinite(velocity, nameof(velocity));
    ValidateFinite(knockback, nameof(knockback));
    ValidateFinite(ai0, nameof(ai0));
    ValidateFinite(ai1, nameof(ai1));
    ValidateFinite(ai2, nameof(ai2));

    OwnerSlot = ownerSlot;
    Identity = identity;
    ProjectileType = projectileType;
    Position = position;
    Velocity = velocity;
    Damage = damage;
    OriginalDamage = originalDamage;
    Knockback = knockback;
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    ProjectileUuid = projectileUuid;
    BannerIdToRespondTo = bannerIdToRespondTo;
  }

  public int OwnerSlot { get; }

  public int Identity { get; }

  public int ProjectileType { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public int Damage { get; }

  public int OriginalDamage { get; }

  public float Knockback { get; }

  public float Ai0 { get; }

  public float Ai1 { get; }

  public float Ai2 { get; }

  public int ProjectileUuid { get; }

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
