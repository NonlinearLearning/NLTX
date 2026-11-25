using System;
using System.Numerics;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Validated input for creating one projectile instance.</summary>
public readonly record struct ProjectileSpawnRequest
{
  public ProjectileSpawnRequest(
      ProjectileId definitionId,
      EntityRuntimeHandle owner,
      ProjectileSourceMetadataComponent source,
      Vector2 position,
      Vector2 velocity,
      ColliderComponent collider,
      EntitySizeComponent size,
      Faction faction)
  {
    if (!definitionId.IsValid)
    {
      throw new ArgumentException("A projectile definition id is required.", nameof(definitionId));
    }

    if (!owner.IsValid)
    {
      throw new ArgumentException("A projectile owner handle is required.", nameof(owner));
    }

    if (string.IsNullOrWhiteSpace(source.SourceKind) ||
        string.IsNullOrWhiteSpace(source.SourceId) || source.SourceRevision < 0)
    {
      throw new ArgumentException("Projectile source metadata is invalid.", nameof(source));
    }

    ValidateFinite(position, nameof(position));
    ValidateFinite(velocity, nameof(velocity));
    if (!Enum.IsDefined(faction))
    {
      throw new ArgumentOutOfRangeException(nameof(faction));
    }

    DefinitionId = definitionId;
    Owner = owner;
    Source = source;
    Position = position;
    Velocity = velocity;
    Collider = collider;
    Size = size;
    Faction = faction;
  }

  public ProjectileId DefinitionId { get; }

  public EntityRuntimeHandle Owner { get; }

  public ProjectileSourceMetadataComponent Source { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public ColliderComponent Collider { get; }

  public EntitySizeComponent Size { get; }

  public Faction Faction { get; }

  private static void ValidateFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
