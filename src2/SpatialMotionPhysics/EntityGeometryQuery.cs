using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public static class EntityGeometryQuery
{
  public static EntityGeometrySnapshot Read(EntitySpatialState state)
  {
    ArgumentNullException.ThrowIfNull(state);

    Vector2 position = state.Position;
    float width = state.Bounds.Width;
    float height = state.Bounds.Height;
    Vector2 center = new(position.X + width / 2f, position.Y + height / 2f);
    Vector2 left = new(position.X, center.Y);
    Vector2 right = new(position.X + width, center.Y);
    Vector2 top = new(center.X, position.Y);
    Vector2 topLeft = position;
    Vector2 topRight = new(position.X + width, position.Y);
    Vector2 bottom = new(center.X, position.Y + height);
    Vector2 bottomLeft = new(position.X, position.Y + height);
    Vector2 bottomRight = new(position.X + width, position.Y + height);

    return new EntityGeometrySnapshot(
      position,
      position,
      center,
      left,
      right,
      top,
      topLeft,
      topRight,
      bottom,
      bottomLeft,
      bottomRight,
      new Vector2(width, height),
      new EntityHitbox(
        (int)position.X,
        (int)position.Y,
        state.Bounds.Width,
        state.Bounds.Height));
  }
}
