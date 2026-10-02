using System;

namespace Terraria.Player.Grapple;

// status: implemented-isolated-core
// crossSubsystemOwner: Projectile identity, lifecycle, network, and persistence
public sealed class PlayerGrappleRelationComponent
{
  public const int SlotCount = 20;

  public const int InvalidProjectileSlot = -1;

  public int[] ProjectileSlots { get; } = new int[SlotCount];

  public int Count { get; set; }

  public PlayerGrappleRelationComponent()
  {
    Array.Fill(ProjectileSlots, InvalidProjectileSlot);
  }
}
