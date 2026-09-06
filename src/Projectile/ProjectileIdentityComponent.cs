using Terraria.Relationships;

namespace Terraria.Projectile;

public struct ProjectileIdentityComponent
{
  public ProjectileIdentityComponent()
  {
    OwnerReference = EntityReference.None;
    SlotIndex = -1;
    Identity = 0;
    ProjectileUuid = -1;
  }

  public ProjectileIdentityComponent(
    EntityReference ownerReference,
    int slotIndex = -1,
    int identity = 0,
    int projectileUuid = -1)
  {
    OwnerReference = ownerReference;
    SlotIndex = slotIndex;
    Identity = identity;
    ProjectileUuid = projectileUuid;
  }

  public EntityReference OwnerReference;
  public int SlotIndex;
  public int Identity;
  public int ProjectileUuid;
}
