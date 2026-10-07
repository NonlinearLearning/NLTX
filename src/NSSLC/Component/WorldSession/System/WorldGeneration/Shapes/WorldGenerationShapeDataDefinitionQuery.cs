using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Provides immutable shape data and pure outline calculations for world generation.
/// </summary>
public static class WorldGenerationShapeDataDefinitionQuery
{
  private const int CardinalOffsetCount = 4;

  private static readonly ImmutableArray<ShapePoint> OutlineOffsets =
    ImmutableArray.Create(
      new ShapePoint(1, 0),
      new ShapePoint(-1, 0),
      new ShapePoint(0, 1),
      new ShapePoint(0, -1),
      new ShapePoint(1, 1),
      new ShapePoint(1, -1),
      new ShapePoint(-1, 1),
      new ShapePoint(-1, -1));

  /// <summary>
  /// Stores a shape coordinate with the same 16-bit coordinate representation as Point16.
  /// </summary>
  public readonly record struct ShapePoint
  {
    public ShapePoint(short x, short y)
    {
      X = x;
      Y = y;
    }

    public ShapePoint(int x, int y)
      : this(unchecked((short)x), unchecked((short)y))
    {
    }

    public short X { get; }

    public short Y { get; }
  }

  /// <summary>
  /// Carries immutable, operation-local shape execution policy.
  /// </summary>
  public readonly record struct ShapeExecutionPolicy(bool QuitOnFail);

  /// <summary>
  /// Owns a deterministic, immutable set of unique shape points.
  /// </summary>
  public sealed class ShapeDataDefinition
  {
    private readonly ImmutableHashSet<ShapePoint> _points;

    public ShapeDataDefinition(IEnumerable<ShapePoint> points)
    {
      ArgumentNullException.ThrowIfNull(points);

      HashSet<ShapePoint> uniquePoints = new();
      foreach (ShapePoint point in points)
      {
        uniquePoints.Add(point);
      }

      List<ShapePoint> orderedPoints = new(uniquePoints);
      orderedPoints.Sort(ComparePoints);
      _points = ImmutableHashSet.CreateRange(uniquePoints);
      Points = ImmutableArray.CreateRange(orderedPoints);
    }

    public int Count => _points.Count;

    public ImmutableArray<ShapePoint> Points { get; }

    public bool Contains(ShapePoint point)
    {
      return _points.Contains(point);
    }
  }

  /// <summary>
  /// Returns an immutable outline result without executing a generation action.
  /// </summary>
  public readonly struct ShapeOutlineResult : IEquatable<ShapeOutlineResult>
  {
    public ShapeOutlineResult(
      ImmutableArray<ShapePoint> points,
      ShapeExecutionPolicy executionPolicy)
    {
      if (points.IsDefault)
      {
        throw new ArgumentException(
          "Shape outline points must be initialized.",
          nameof(points));
      }

      Points = points;
      ExecutionPolicy = executionPolicy;
    }

    public ImmutableArray<ShapePoint> Points { get; }

    public ShapeExecutionPolicy ExecutionPolicy { get; }

    public bool Equals(ShapeOutlineResult other)
    {
      if (ExecutionPolicy != other.ExecutionPolicy ||
        Points.Length != other.Points.Length)
      {
        return false;
      }

      for (int index = 0; index < Points.Length; index++)
      {
        if (Points[index] != other.Points[index])
        {
          return false;
        }
      }

      return true;
    }

    public override bool Equals(object? obj)
    {
      return obj is ShapeOutlineResult other && Equals(other);
    }

    public override int GetHashCode()
    {
      HashCode hash = new();
      hash.Add(ExecutionPolicy);
      for (int index = 0; index < Points.Length; index++)
      {
        hash.Add(Points[index]);
      }

      return hash.ToHashCode();
    }

    public static bool operator ==(
      ShapeOutlineResult left,
      ShapeOutlineResult right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      ShapeOutlineResult left,
      ShapeOutlineResult right)
    {
      return !left.Equals(right);
    }
  }

  public static int Count(ShapeDataDefinition shape)
  {
    ArgumentNullException.ThrowIfNull(shape);
    return shape.Count;
  }

  public static ShapeOutlineResult OuterOutline(
    ShapeDataDefinition shape,
    bool useDiagonals,
    bool useInterior,
    ShapeExecutionPolicy executionPolicy)
  {
    ArgumentNullException.ThrowIfNull(shape);

    HashSet<ShapePoint> outlinePoints = new();
    if (useInterior)
    {
      AddPoints(outlinePoints, shape.Points);
    }

    int offsetCount = useDiagonals
      ? OutlineOffsets.Length
      : CardinalOffsetCount;
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      ShapePoint point = shape.Points[pointIndex];
      for (int offsetIndex = 0; offsetIndex < offsetCount; offsetIndex++)
      {
        ShapePoint candidate = Add(point, OutlineOffsets[offsetIndex]);
        if (!shape.Contains(candidate))
        {
          outlinePoints.Add(candidate);
        }
      }
    }

    return CreateResult(outlinePoints, executionPolicy);
  }

  public static ShapeOutlineResult InnerOutline(
    ShapeDataDefinition shape,
    bool useDiagonals,
    ShapeExecutionPolicy executionPolicy)
  {
    ArgumentNullException.ThrowIfNull(shape);

    HashSet<ShapePoint> outlinePoints = new();
    int offsetCount = useDiagonals
      ? OutlineOffsets.Length
      : CardinalOffsetCount;
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      ShapePoint point = shape.Points[pointIndex];
      bool hasMissingNeighbor = false;
      for (int offsetIndex = 0; offsetIndex < offsetCount; offsetIndex++)
      {
        ShapePoint candidate = Add(point, OutlineOffsets[offsetIndex]);
        if (!shape.Contains(candidate))
        {
          hasMissingNeighbor = true;
          break;
        }
      }

      if (hasMissingNeighbor)
      {
        outlinePoints.Add(point);
      }
    }

    return CreateResult(outlinePoints, executionPolicy);
  }

  private static void AddPoints(
    HashSet<ShapePoint> destination,
    ImmutableArray<ShapePoint> points)
  {
    for (int index = 0; index < points.Length; index++)
    {
      destination.Add(points[index]);
    }
  }

  private static ShapePoint Add(ShapePoint left, ShapePoint right)
  {
    return new ShapePoint(left.X + right.X, left.Y + right.Y);
  }

  private static ShapeOutlineResult CreateResult(
    HashSet<ShapePoint> points,
    ShapeExecutionPolicy executionPolicy)
  {
    List<ShapePoint> orderedPoints = new(points);
    orderedPoints.Sort(ComparePoints);
    return new ShapeOutlineResult(
      ImmutableArray.CreateRange(orderedPoints),
      executionPolicy);
  }

  private static int ComparePoints(ShapePoint left, ShapePoint right)
  {
    int compareX = left.X.CompareTo(right.X);
    return compareX != 0
      ? compareX
      : left.Y.CompareTo(right.Y);
  }
}
