using System;

using Terraria.Relationships;

namespace Terraria.Projectile;

public struct ProjectileIdentityComponent
{
  public ProjectileIdentityComponent()
  {
    OwnerReference = EntityReference.None;
    OwnerSlot = byte.MaxValue;
    SlotIndex = -1;
    Identity = 0;
    ProjectileUuid = -1;
  }

  public ProjectileIdentityComponent(
    EntityReference ownerReference,
    int slotIndex = -1,
    int identity = 0,
    int projectileUuid = -1,
    int ownerSlot = byte.MaxValue)
  {
    if (slotIndex < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }

    if (identity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    if (projectileUuid < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileUuid));
    }

    if ((uint)ownerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerSlot));
    }

    OwnerReference = ownerReference;
    OwnerSlot = ownerSlot;
    SlotIndex = slotIndex;
    Identity = identity;
    ProjectileUuid = projectileUuid;
  }

  public EntityReference OwnerReference;
  public int OwnerSlot;
  public int SlotIndex;
  public int Identity;
  public int ProjectileUuid;

  public ProjectileOwnerReference Owner =>
    new(OwnerReference, OwnerSlot);

  public bool HasProjectileSlot => SlotIndex >= 0;

  public bool HasProjectileUuid => ProjectileUuid >= 0;

  public bool MatchesOwnerAndIdentity(int ownerSlot, int identity)
  {
    return (uint)ownerSlot <= byte.MaxValue &&
      identity >= 0 &&
      OwnerSlot == ownerSlot &&
      Identity == identity;
  }

  public bool MatchesNetworkIdentity(
    int ownerSlot,
    int identity,
    int projectileUuid)
  {
    if (!MatchesOwnerAndIdentity(ownerSlot, identity))
    {
      return false;
    }

    return projectileUuid < 0 ||
      ProjectileUuid < 0 ||
      ProjectileUuid == projectileUuid;
  }
}
