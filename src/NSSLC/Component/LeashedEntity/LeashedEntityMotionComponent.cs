using System.Numerics;
using EntityEcs.Components;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores shared geometric state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedEntityMotionComponent
{
  /// <summary>
  /// Position candidate mapped to the existing NLTX component.
  /// </summary>
  public LocationComponent Position;

  /// <summary>
  /// Velocity candidate mapped to the existing NLTX component.
  /// </summary>
  public VelocityComponent Velocity;

  /// <summary>
  /// Horizontal direction candidate mapped to the existing NLTX component.
  /// </summary>
  public DirectionComponent Direction;

  /// <summary>
  /// Width in the shared geometry unit.
  /// </summary>
  public int Width;

  /// <summary>
  /// Height in the shared geometry unit.
  /// </summary>
  public int Height;

  /// <summary>
  /// Derived geometry; do not store or network-sync as another authority.
  /// </summary>
  public Vector2 Center => new(
    Position.X + Width / 2.0f,
    Position.Y + Height / 2.0f);

  /// <summary>
  /// Derived geometry; do not store or network-sync as another authority.
  /// </summary>
  public Vector2 Size => new(Width, Height);

  public LeashedEntityMotionComponent(
    LocationComponent position,
    VelocityComponent velocity,
    DirectionComponent direction,
    int width,
    int height)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Width = width;
    Height = height;
  }
}
