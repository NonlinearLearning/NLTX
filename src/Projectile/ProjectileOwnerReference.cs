using System;

using Terraria.Relationships;

namespace Terraria.Projectile;

public readonly record struct ProjectileOwnerReference
{
  public ProjectileOwnerReference(
    EntityReference entityReference,
    int legacyOwnerSlot)
  {
    if ((uint)legacyOwnerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(legacyOwnerSlot));
    }

    EntityReference = entityReference;
    LegacyOwnerSlot = legacyOwnerSlot;
  }

  public static ProjectileOwnerReference None =>
    new(EntityReference.None, byte.MaxValue);

  public EntityReference EntityReference { get; }

  public int LegacyOwnerSlot { get; }

  public bool HasEntityReference => !EntityReference.IsEmpty;
}
