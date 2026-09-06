using System;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_SHAPE
// crossSubsystemOwner: integration-review
public readonly record struct CollisionShapeComponent
{
  public CollisionShapeComponent(
    float width,
    float height,
    float offsetX = 0.0f,
    float offsetY = 0.0f,
    CollisionShapeKind kind = CollisionShapeKind.Rectangle,
    bool isEnabled = true)
  {
    EnsureFinite(width, nameof(width));
    EnsureFinite(height, nameof(height));
    EnsureFinite(offsetX, nameof(offsetX));
    EnsureFinite(offsetY, nameof(offsetY));

    if (width < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(width),
        width,
        "Width must not be negative.");
    }

    if (height < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(height),
        height,
        "Height must not be negative.");
    }

    Width = width;
    Height = height;
    OffsetX = offsetX;
    OffsetY = offsetY;
    Kind = kind;
    IsEnabled = isEnabled;
  }

  public float Width { get; }
  public float Height { get; }
  public float OffsetX { get; }
  public float OffsetY { get; }
  public CollisionShapeKind Kind { get; }
  public bool IsEnabled { get; }

  public bool HasArea =>
    IsEnabled && Width > 0.0f && Height > 0.0f;

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Value must be finite.");
    }
  }
}
