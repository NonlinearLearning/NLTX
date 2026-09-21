using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Applies immutable shape modifiers using explicit inputs and no hidden generation state.
/// </summary>
public static class WorldGenerationShapeModifierStateDefinitionQuery
{
  public readonly record struct ShapeScaleDefinition(int Scale);

  public readonly record struct ExpandDefinition(
    int XExpansion,
    int YExpansion);

  public readonly record struct RadialDitherDefinition(
    double InnerRadius,
    double OuterRadius);

  public readonly record struct BlotchesDefinition(
    int MinX,
    int MinY,
    int MaxX,
    int MaxY,
    double Chance);

  public readonly record struct ShapeMembershipDefinition(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Shape);

  public readonly record struct CheckerboardDefinition(int Percentile);

  public readonly record struct RectangleMaskDefinition(
    int XMin,
    int YMin,
    int XMax,
    int YMax);

  public readonly record struct OffsetDefinition(int XOffset, int YOffset);

  public readonly record struct DitherDefinition(double FailureChance);

  public readonly record struct FlipDefinition(bool FlipX, bool FlipY);

  /// <summary>
  /// Supplies the random value used by a radial dither decision.
  /// </summary>
  public readonly record struct RadialDitherRandomInput(double Value);

  /// <summary>
  /// Supplies all random decisions for one blotch invocation.
  /// </summary>
  public readonly record struct BlotchesRandomInput(
    double ActivationValue,
    int LeftOffset,
    int RightOffset,
    int UpOffset,
    int DownOffset);

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Scale(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    ShapeScaleDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ValidateNonNegative(definition.Scale, nameof(definition.Scale));

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point =
        shape.Points[pointIndex];
      for (int x = 0; x < definition.Scale; x++)
      {
        for (int y = 0; y < definition.Scale; y++)
        {
          points.Add(
            new WorldGenerationShapeDataDefinitionQuery.ShapePoint(
              (point.X * 2) + x,
              (point.Y * 2) + y));
        }
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Expand(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    ExpandDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ValidateNonNegative(definition.XExpansion, nameof(definition.XExpansion));
    ValidateNonNegative(definition.YExpansion, nameof(definition.YExpansion));

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point =
        shape.Points[pointIndex];
      for (int x = -definition.XExpansion; x <= definition.XExpansion; x++)
      {
        for (int y = -definition.YExpansion; y <= definition.YExpansion; y++)
        {
          points.Add(
            new WorldGenerationShapeDataDefinitionQuery.ShapePoint(
              point.X + x,
              point.Y + y));
        }
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition RadialDither(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    RadialDitherDefinition definition,
    RadialDitherRandomInput randomInput)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ValidateRadius(definition);
    ValidateUnit(randomInput.Value, nameof(randomInput.Value));

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point =
        shape.Points[pointIndex];
      double distance = Math.Sqrt(
        ((double)point.X * point.X) +
        ((double)point.Y * point.Y));
      double threshold = Math.Clamp(
        (distance - definition.InnerRadius) /
        (definition.OuterRadius - definition.InnerRadius),
        0,
        1);
      if (randomInput.Value > threshold)
      {
        points.Add(point);
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Blotches(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    BlotchesDefinition definition,
    BlotchesRandomInput randomInput)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ValidateBlotches(definition, randomInput);

    if (randomInput.ActivationValue >= definition.Chance)
    {
      return new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(
        shape.Points);
    }

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int pointIndex = 0; pointIndex < shape.Points.Length; pointIndex++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point =
        shape.Points[pointIndex];
      for (int x = randomInput.LeftOffset;
        x <= randomInput.RightOffset;
        x++)
      {
        for (int y = randomInput.UpOffset;
          y <= randomInput.DownOffset;
          y++)
        {
          points.Add(
            new WorldGenerationShapeDataDefinitionQuery.ShapePoint(
              point.X + x,
              point.Y + y));
        }
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition InShape(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    ShapeMembershipDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ArgumentNullException.ThrowIfNull(definition.Shape);
    return FilterByMembership(shape, definition.Shape, expectedMembership: true);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition NotInShape(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    ShapeMembershipDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ArgumentNullException.ThrowIfNull(definition.Shape);
    return FilterByMembership(shape, definition.Shape, expectedMembership: false);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Checkerboard(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    CheckerboardDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    if (definition.Percentile <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definition.Percentile));
    }

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int index = 0; index < shape.Points.Length; index++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point = shape.Points[index];
      if (point.X % definition.Percentile == 0 &&
        point.Y % definition.Percentile == 0)
      {
        points.Add(point);
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition RectangleMask(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    RectangleMaskDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);
    if (definition.XMin > definition.XMax)
    {
      throw new ArgumentException(
        "The rectangle minimum X must not exceed its maximum X.",
        nameof(definition));
    }

    if (definition.YMin > definition.YMax)
    {
      throw new ArgumentException(
        "The rectangle minimum Y must not exceed its maximum Y.",
        nameof(definition));
    }

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int index = 0; index < shape.Points.Length; index++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point = shape.Points[index];
      if (point.X >= definition.XMin && point.X <= definition.XMax &&
        point.Y >= definition.YMin && point.Y <= definition.YMax)
      {
        points.Add(point);
      }
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Offset(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    OffsetDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int index = 0; index < shape.Points.Length; index++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point = shape.Points[index];
      points.Add(
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(
          point.X + definition.XOffset,
          point.Y + definition.YOffset));
    }

    return CreateShape(points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Dither(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    DitherDefinition definition,
    double randomValue)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ValidateUnit(randomValue, nameof(randomValue));
    ValidateUnit(definition.FailureChance, nameof(definition.FailureChance));

    if (randomValue < definition.FailureChance)
    {
      return new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(
        Array.Empty<WorldGenerationShapeDataDefinitionQuery.ShapePoint>());
    }

    return new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(shape.Points);
  }

  public static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition Flip(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    FlipDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(shape);

    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int index = 0; index < shape.Points.Length; index++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point = shape.Points[index];
      int x = definition.FlipX ? -point.X : point.X;
      int y = definition.FlipY ? -point.Y : point.Y;
      points.Add(
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(x, y));
    }

    return CreateShape(points);
  }

  private static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition
    FilterByMembership(
      WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
      WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition membershipShape,
      bool expectedMembership)
  {
    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = new();
    for (int index = 0; index < shape.Points.Length; index++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint point = shape.Points[index];
      if (membershipShape.Contains(point) == expectedMembership)
      {
        points.Add(point);
      }
    }

    return CreateShape(points);
  }

  private static WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition CreateShape(
    HashSet<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points)
  {
    return new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(points);
  }

  private static void ValidateBlotches(
    BlotchesDefinition definition,
    BlotchesRandomInput randomInput)
  {
    ValidateNonNegative(definition.MinX, nameof(definition.MinX));
    ValidateNonNegative(definition.MinY, nameof(definition.MinY));
    ValidateNonNegative(definition.MaxX, nameof(definition.MaxX));
    ValidateNonNegative(definition.MaxY, nameof(definition.MaxY));
    ValidateUnit(definition.Chance, nameof(definition.Chance));
    ValidateUnit(randomInput.ActivationValue, nameof(randomInput.ActivationValue));
    if (randomInput.LeftOffset > randomInput.RightOffset ||
      randomInput.UpOffset > randomInput.DownOffset)
    {
      throw new ArgumentException(
        "Blotch random offsets must describe a non-empty rectangle.",
        nameof(randomInput));
    }

    if (randomInput.LeftOffset < 1 - definition.MinX ||
      randomInput.LeftOffset > 0 ||
      randomInput.RightOffset < 0 ||
      randomInput.RightOffset >= definition.MaxX ||
      randomInput.UpOffset < 1 - definition.MinY ||
      randomInput.UpOffset > 0 ||
      randomInput.DownOffset < 0 ||
      randomInput.DownOffset >= definition.MaxY)
    {
      throw new ArgumentException(
        "Blotch random offsets are outside the configured source ranges.",
        nameof(randomInput));
    }
  }

  private static void ValidateRadius(RadialDitherDefinition definition)
  {
    if (double.IsNaN(definition.InnerRadius) ||
      double.IsInfinity(definition.InnerRadius) ||
      definition.InnerRadius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definition.InnerRadius));
    }

    if (double.IsNaN(definition.OuterRadius) ||
      double.IsInfinity(definition.OuterRadius) ||
      definition.OuterRadius <= definition.InnerRadius)
    {
      throw new ArgumentOutOfRangeException(nameof(definition.OuterRadius));
    }
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidateUnit(double value, string parameterName)
  {
    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 1)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
