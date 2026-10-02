using Terraria.Relationships;

namespace Terraria.Projectile;

public struct ProjectileOwnerComponent
{
  public ProjectileOwnerComponent(EntityReference owner)
  {
    Owner = owner;
  }

  public EntityReference Owner;
}
