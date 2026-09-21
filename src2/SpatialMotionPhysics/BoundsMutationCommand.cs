using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class BoundsMutationCommand
{
  public BoundsMutationCommand(BoundsAnchor anchor, Vector2 position)
    : this(anchor, position, null)
  {
  }

  public BoundsMutationCommand(
    BoundsAnchor anchor,
    Vector2 position,
    Vector2? size)
  {
    Anchor = anchor;
    Position = position;
    Size = size;
  }

  public BoundsAnchor Anchor { get; }

  public Vector2 Position { get; }

  public Vector2? Size { get; }
}
