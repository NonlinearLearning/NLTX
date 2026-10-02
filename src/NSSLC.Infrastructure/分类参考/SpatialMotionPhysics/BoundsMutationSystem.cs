using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public static class BoundsMutationSystem
{
  public static void Apply(
    EntitySpatialState state,
    BoundsMutationCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(command);

    EntityBoundsComponent bounds = state.Bounds;
    Vector2 position = state.Position;
    int width = bounds.Width;
    int height = bounds.Height;

    if (command.Size is Vector2 requestedSize)
    {
      width = (int)requestedSize.X;
      height = (int)requestedSize.Y;
    }

    switch (command.Anchor)
    {
      case BoundsAnchor.Center:
        position = new Vector2(
          command.Position.X - width / 2f,
          command.Position.Y - height / 2f);
        break;
      case BoundsAnchor.Left:
        position = new Vector2(
          command.Position.X,
          command.Position.Y - height / 2f);
        break;
      case BoundsAnchor.Right:
        position = new Vector2(
          command.Position.X - width,
          command.Position.Y - height / 2f);
        break;
      case BoundsAnchor.Top:
        position = new Vector2(
          command.Position.X - width / 2f,
          command.Position.Y);
        break;
      case BoundsAnchor.TopLeft:
        position = command.Position;
        break;
      case BoundsAnchor.TopRight:
        position = new Vector2(command.Position.X - width, command.Position.Y);
        break;
      case BoundsAnchor.Bottom:
        position = new Vector2(
          command.Position.X - width / 2f,
          command.Position.Y - height);
        break;
      case BoundsAnchor.BottomLeft:
        position = new Vector2(command.Position.X, command.Position.Y - height);
        break;
      case BoundsAnchor.BottomRight:
        position = new Vector2(
          command.Position.X - width,
          command.Position.Y - height);
        break;
      case BoundsAnchor.Size:
        break;
      case BoundsAnchor.Hitbox:
        position = command.Position;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(command.Anchor));
    }

    bounds.SetSize(width, height);
    state.Position = position;
  }
}
