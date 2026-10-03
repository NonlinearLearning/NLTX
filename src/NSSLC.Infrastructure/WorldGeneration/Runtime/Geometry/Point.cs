using System;

namespace NSSLC.WorldGeneration.Geometry;

public struct Point : IEquatable<Point> {
  public int X;
  public int Y;
  public static Point Zero => new(0, 0);
  public Point(int x, int y) {
    X = x;
    Y = y;
  }
  public bool Equals(Point other) => X == other.X && Y == other.Y;
  public override bool Equals(object obj) => obj is Point other && Equals(other);
  public override int GetHashCode() => HashCode.Combine(X, Y);
  public static Point operator +(Point first, Point second) => new(first.X + second.X, first.Y + second.Y);
  public static Point operator -(Point first, Point second) => new(first.X - second.X, first.Y - second.Y);
  public static bool operator ==(Point first, Point second) => first.Equals(second);
  public static bool operator !=(Point first, Point second) => !first.Equals(second);
}
