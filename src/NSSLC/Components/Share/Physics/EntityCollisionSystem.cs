using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Terraria.Entity;

namespace Terraria.Physics;

/// <summary>
/// Performs deterministic broad-phase AABB contact checks over detached spatial bodies.
/// It never applies damage, knockback or component writes.
/// </summary>
public sealed class EntityCollisionSystem
{
  public IReadOnlyList<SpatialContactPair<TEntity>> FindContacts<TEntity>(
      IReadOnlyList<SpatialBody<TEntity>> bodies)
      where TEntity : notnull
  {
    ArgumentNullException.ThrowIfNull(bodies);
    var contacts = new List<SpatialContactPair<TEntity>>();
    for (int firstIndex = 0; firstIndex < bodies.Count; firstIndex++)
    {
      SpatialBody<TEntity> first = bodies[firstIndex];
      if (!first.Collision.Enabled)
      {
        continue;
      }

      for (int secondIndex = firstIndex + 1; secondIndex < bodies.Count; secondIndex++)
      {
        SpatialBody<TEntity> second = bodies[secondIndex];
        if (TryGetContact(
                first.Snapshot.Location,
                first.Snapshot.Collider,
                first.Collision,
                second.Snapshot.Location,
                second.Snapshot.Collider,
                second.Collision,
                out EntityCollisionContact contact))
        {
          contacts.Add(new SpatialContactPair<TEntity>(
              first.Snapshot.Entity,
              second.Snapshot.Entity,
              contact));
        }
      }
    }

    return new ReadOnlyCollection<SpatialContactPair<TEntity>>(contacts);
  }

  /// <summary>
  /// Tests two entity colliders using the shared capability and AABB rules.
  /// </summary>
  public bool TryGetContact(
      LocationComponent firstLocation,
      ColliderComponent firstCollider,
      EntityCollisionCapabilityComponent firstCollision,
      LocationComponent secondLocation,
      ColliderComponent secondCollider,
      EntityCollisionCapabilityComponent secondCollision,
      out EntityCollisionContact contact)
  {
    if (!firstCollision.Enabled || !secondCollision.Enabled ||
        !firstCollider.Enabled || !secondCollider.Enabled ||
        !secondCollision.CanCollideWith(firstCollision.Layer) ||
        !firstCollision.CanCollideWith(secondCollision.Layer))
    {
      contact = default;
      return false;
    }

    Vector2 firstMinimum = firstLocation.Coordinates + firstCollider.Offset;
    Vector2 firstMaximum = firstMinimum + firstCollider.Size;
    Vector2 secondMinimum = secondLocation.Coordinates + secondCollider.Offset;
    Vector2 secondMaximum = secondMinimum + secondCollider.Size;
    float overlapX = MathF.Min(firstMaximum.X, secondMaximum.X) -
        MathF.Max(firstMinimum.X, secondMinimum.X);
    float overlapY = MathF.Min(firstMaximum.Y, secondMaximum.Y) -
        MathF.Max(firstMinimum.Y, secondMinimum.Y);
    if (overlapX <= 0f || overlapY <= 0f)
    {
      contact = default;
      return false;
    }

    Vector2 normal = overlapX < overlapY
        ? new Vector2(
            firstMinimum.X < secondMinimum.X ? -1f : 1f,
            0f)
        : new Vector2(
            0f,
            firstMinimum.Y < secondMinimum.Y ? -1f : 1f);
    Vector2 point = Vector2.Lerp(
        firstMinimum + firstCollider.Size / 2f,
        secondMinimum + secondCollider.Size / 2f,
        0.5f);
    contact = new EntityCollisionContact(point, normal);
    return true;
  }
}
