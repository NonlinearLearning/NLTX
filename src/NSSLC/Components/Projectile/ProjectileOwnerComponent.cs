using System;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Stores the owning entity handle used for attribution and collision exclusion.</summary>
public readonly record struct ProjectileOwnerComponent
{
  public ProjectileOwnerComponent(EntityRuntimeHandle owner)
  {
    if (!owner.IsValid)
    {
      throw new ArgumentException("A projectile owner handle is required.", nameof(owner));
    }

    Owner = owner;
  }

  public EntityRuntimeHandle Owner { get; }
}
