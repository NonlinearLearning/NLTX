using System.Collections.ObjectModel;
using System.Numerics;

namespace Terraria.Combat.Targeting;

public sealed class MultiPointHitboxSnapshot
{
  private readonly Vector2[] _points;
  private readonly ReadOnlyCollection<Vector2> _readOnlyPoints;

  public MultiPointHitboxSnapshot(Vector2 pointSize, IReadOnlyList<Vector2> points)
  {
    ArgumentNullException.ThrowIfNull(points);
    if (points.Count == 0)
    {
      throw new ArgumentException("At least one hitbox point is required.", nameof(points));
    }

    PointSize = pointSize;
    _points = points.ToArray();
    _readOnlyPoints = Array.AsReadOnly(_points);
    BoundingRect = CreateBoundingRect(_points);
  }

  public Vector2 PointSize { get; }

  public IReadOnlyList<Vector2> Points => _readOnlyPoints;

  public CombatRectangle BoundingRect { get; }

  private static CombatRectangle CreateBoundingRect(IReadOnlyList<Vector2> points)
  {
    float minX = points[0].X;
    float maxX = points[0].X;
    float minY = points[0].Y;
    float maxY = points[0].Y;

    for (int index = 1; index < points.Count; index++)
    {
      Vector2 point = points[index];
      minX = MathF.Min(minX, point.X);
      maxX = MathF.Max(maxX, point.X);
      minY = MathF.Min(minY, point.Y);
      maxY = MathF.Max(maxY, point.Y);
    }

    return new CombatRectangle(minX, minY, maxX - minX, maxY - minY);
  }
}
