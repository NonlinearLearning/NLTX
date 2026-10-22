using System.Numerics;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Trusted local NewProjectile inputs for the migrated neutral-source ordinary Arrow path.
/// Owner -1 is normalized to the server sentinel 255.
/// </summary>
public sealed class ProjectileLocalArrowSpawnRequest
{
  public ProjectileLocalArrowSpawnRequest(
      Vector2 center,
      Vector2 velocity,
      int damage,
      float knockback,
      int owner,
      float ai0,
      float ai1,
      float ai2,
      int currentLocalPlayerSlot,
      bool networked,
      IProjectileArrowSpawnSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    Center = center;
    Velocity = velocity;
    Damage = damage;
    Knockback = knockback;
    Owner = owner;
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    CurrentLocalPlayerSlot = currentLocalPlayerSlot;
    Networked = networked;
    Snapshot = snapshot;
  }

  public Vector2 Center { get; }

  public Vector2 Velocity { get; }

  public int Damage { get; }

  public float Knockback { get; }

  public int Owner { get; }

  public float Ai0 { get; }

  public float Ai1 { get; }

  public float Ai2 { get; }

  /// <summary>
  /// The captured Main.myPlayer value; this path requires the server sentinel 255.
  /// </summary>
  public int CurrentLocalPlayerSlot { get; }

  /// <summary>Whether source NewProjectile would emit packet 27 for this local owner.</summary>
  public bool Networked { get; }

  public IProjectileArrowSpawnSnapshot Snapshot { get; }
}
