namespace EntityEcs.Components;

public readonly record struct ColliderComponent
{
  public ColliderComponent()
    : this(0.0f, 0.0f)
  {
  }

  public ColliderComponent(
    float width,
    float height,
    float offsetX = 0.0f,
    float offsetY = 0.0f,
    CollisionShapeKind kind = CollisionShapeKind.Rectangle,
    bool isEnabled = true)
  {
    Width = width;
    Height = height;
    OffsetX = offsetX;
    OffsetY = offsetY;
    Kind = kind;
    IsEnabled = isEnabled;
  }

  public float Height { get; }
  public bool HasArea => Width > 0.0f && Height > 0.0f;
  public bool IsEnabled { get; }
  public CollisionShapeKind Kind { get; }
  public float OffsetX { get; }
  public float OffsetY { get; }
  public float Width { get; }
}
