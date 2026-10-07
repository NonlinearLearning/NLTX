using System.Numerics;

namespace Terraria.Player.Mobility;

public readonly record struct PlayerGrappleProjectileSnapshot(
  int Type,
  float Ai0,
  Vector2 Position,
  int Width,
  int Height,
  float Rotation)
{
  public Vector2 Center => Position + new Vector2(Width / 2, Height / 2);

  public bool HasNaNPosition => float.IsNaN(Position.X) || float.IsNaN(Position.Y);
}
