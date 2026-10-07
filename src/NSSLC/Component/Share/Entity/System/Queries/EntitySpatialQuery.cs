using System;
using System.Numerics;
using EntityEcs.Components;

namespace EntityEcs.Queries;

public static class EntitySpatialQuery
{
  public static float AngleTo(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 destination)
  {
    Vector2 center = EntityGeometryQuery.Center(transform, collider);
    return MathF.Atan2(destination.Y - center.Y, destination.X - center.X);
  }

  public static float AngleFrom(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 source)
  {
    Vector2 center = EntityGeometryQuery.Center(transform, collider);
    return MathF.Atan2(center.Y - source.Y, center.X - source.X);
  }

  public static float DistanceSquared(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 other)
  {
    return Vector2.DistanceSquared(EntityGeometryQuery.Center(transform, collider), other);
  }

  public static float Distance(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 other)
  {
    return MathF.Sqrt(DistanceSquared(transform, collider, other));
  }

  public static Vector2 DirectionTo(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 destination)
  {
    return Vector2.Normalize(destination - EntityGeometryQuery.Center(transform, collider));
  }

  public static Vector2 DirectionFrom(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 source)
  {
    return Vector2.Normalize(EntityGeometryQuery.Center(transform, collider) - source);
  }

  public static bool WithinRange(
    LocationComponent transform,
    ColliderComponent collider,
    Vector2 target,
    float maxRange)
  {
    return DistanceSquared(transform, collider, target) <= maxRange * maxRange;
  }
}
