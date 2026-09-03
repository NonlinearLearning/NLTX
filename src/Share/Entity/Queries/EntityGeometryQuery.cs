using System.Numerics;
using EntityEcs.Components;

namespace EntityEcs.Queries;

public static class EntityGeometryQuery
{
  public static Vector2 Center(
    LocationComponent transform,
    ColliderComponent collider)
  {
    return new Vector2(
      transform.X + collider.Width / 2.0f,
      transform.Y + collider.Height / 2.0f);
  }

  public static Vector2 Size(ColliderComponent collider)
  {
    return new Vector2(collider.Width, collider.Height);
  }

  public static Vector2 Left(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X, transform.Y + collider.Height / 2.0f);
  }

  public static Vector2 Right(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X + collider.Width, transform.Y + collider.Height / 2.0f);
  }

  public static Vector2 Top(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X + collider.Width / 2.0f, transform.Y);
  }

  public static Vector2 TopLeft(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X, transform.Y);
  }

  public static Vector2 TopRight(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X + collider.Width, transform.Y);
  }

  public static Vector2 Bottom(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X + collider.Width / 2.0f, transform.Y + collider.Height);
  }

  public static Vector2 BottomLeft(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X, transform.Y + collider.Height);
  }

  public static Vector2 BottomRight(LocationComponent transform, ColliderComponent collider)
  {
    return new Vector2(transform.X + collider.Width, transform.Y + collider.Height);
  }

  public static EntityHitbox Hitbox(
    LocationComponent transform,
    ColliderComponent collider)
  {
    return new EntityHitbox(
      (int)transform.X,
      (int)transform.Y,
      (int)collider.Width,
      (int)collider.Height);
  }
}
