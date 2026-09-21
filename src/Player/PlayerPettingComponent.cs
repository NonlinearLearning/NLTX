using System.Numerics;

namespace Terraria.Player;

// Stores the player's petting phase and target compatibility facts.
// NPC, Projectile and Mount registry validation remains outside the component.
public sealed class PlayerPettingComponent
{
  public bool IsPetting { get; set; }

  public int NpcSlot { get; set; } = -1;

  public int ExpectedNpcType { get; set; } = -1;

  public int ProjectileSlot { get; set; } = -1;

  public int ExpectedProjectileType { get; set; } = -1;

  public int MountId { get; set; } = -1;

  public bool IsMountTarget { get; set; }

  public Vector2 OffsetFromPet { get; set; }

  public bool IsPetSmall { get; set; }
}
