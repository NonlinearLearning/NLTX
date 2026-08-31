using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonQuadCircleShape
{
  private const int CenterAdjustment = 3;

  private readonly int _radius;
  private readonly int _distanceBetweenSpheres;

  public DungeonQuadCircleShape(int radius, int distanceBetweenSpheres)
  {
    if (radius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(radius));
    }

    _radius = radius;
    _distanceBetweenSpheres = distanceBetweenSpheres;
  }

  public int Radius => _radius;

  public int DistanceBetweenSpheres => _distanceBetweenSpheres;

  public bool Contains(int horizontalOffset, int verticalOffset)
  {
    int centerOffset = _distanceBetweenSpheres - CenterAdjustment;
    return ContainsCircle(horizontalOffset, verticalOffset, 0, -centerOffset) ||
      ContainsCircle(horizontalOffset, verticalOffset, 0, centerOffset) ||
      ContainsCircle(horizontalOffset, verticalOffset, -centerOffset, 0) ||
      ContainsCircle(horizontalOffset, verticalOffset, centerOffset, 0) ||
      ContainsCircle(horizontalOffset, verticalOffset, 0, 0);
  }

  private bool ContainsCircle(int horizontalOffset, int verticalOffset, int centerX, int centerY)
  {
    int relativeX = horizontalOffset - centerX;
    int relativeY = verticalOffset - centerY;
    if (relativeY < -_radius || relativeY > _radius)
    {
      return false;
    }

    double radiusPlusOne = _radius + 1d;
    double extentSquared = radiusPlusOne * radiusPlusOne - (double)relativeY * relativeY;
    int horizontalExtent = Math.Min(_radius, (int)Math.Sqrt(Math.Max(0d, extentSquared)));
    return relativeX >= -horizontalExtent && relativeX <= horizontalExtent;
  }
}
