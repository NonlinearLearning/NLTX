using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Generation-bound internal identity plus the Version4 tracker wire tuple.</summary>
public sealed record ProjectileTrackerReference
{
  public ProjectileTrackerReference(
    CombatEntityIdentity entity,
    short owner,
    short identity,
    short type)
  {
    if (!entity.IsValid)
    {
      throw new ArgumentException("A tracker must bind to a valid projectile generation.", nameof(entity));
    }

    if (owner is < byte.MinValue or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(owner));
    }

    if (type <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }

    Entity = entity;
    Owner = owner;
    Identity = identity;
    Type = type;
  }

  public CombatEntityIdentity Entity { get; }

  public short Owner { get; }

  public short Identity { get; }

  public short Type { get; }
}
