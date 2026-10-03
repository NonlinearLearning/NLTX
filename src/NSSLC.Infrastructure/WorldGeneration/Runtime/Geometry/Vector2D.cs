using System;

namespace NSSLC.WorldGeneration.Geometry;

public struct Vector2D : IEquatable<Vector2D> {
  public double X;
  public double Y;
  public static Vector2D Zero => new(0, 0);
  public static Vector2D One => new(1, 1);
  public static Vector2D UnitX => new(1, 0);
  public static Vector2D UnitY => new(0, 1);

  public Vector2D(double x, double y) {
    X = x;
    Y = y;
  }
  public Vector2D(double value) : this(value, value) { }
  public double LengthSquared() => X * X + Y * Y;
  public double Length() => Math.Sqrt(LengthSquared());
  public void Normalize() {
    double scale = 1.0 / Length();
    X *= scale;
    Y *= scale;
  }
  public static Vector2D Normalize(Vector2D value) {
    value.Normalize();
    return value;
  }
  public static double Distance(Vector2D value1, Vector2D value2) => (value1 - value2).Length();
  public static double DistanceSquared(Vector2D first, Vector2D second) => (first - second).LengthSquared();
  public static double Dot(Vector2D first, Vector2D second) => first.X * second.X + first.Y * second.Y;
  public static double Cross(Vector2D first, Vector2D second) => first.X * second.Y - first.Y * second.X;
  public static Vector2D Lerp(Vector2D first, Vector2D second, double amount) => first + (second - first) * amount;
  public static Vector2D Clamp(Vector2D value, Vector2D minimum, Vector2D maximum) =>
    new(Math.Clamp(value.X, minimum.X, maximum.X), Math.Clamp(value.Y, minimum.Y, maximum.Y));
  public bool Equals(Vector2D other) => X == other.X && Y == other.Y;
  public override bool Equals(object obj) => obj is Vector2D other && Equals(other);
  public override int GetHashCode() => HashCode.Combine(X, Y);
  public static Vector2D operator +(Vector2D first, Vector2D second) => new(first.X + second.X, first.Y + second.Y);
  public static Vector2D operator -(Vector2D first, Vector2D second) => new(first.X - second.X, first.Y - second.Y);
  public static Vector2D operator -(Vector2D value) => new(-value.X, -value.Y);
  public static Vector2D operator *(Vector2D value, double scale) => new(value.X * scale, value.Y * scale);
  public static Vector2D operator *(double scale, Vector2D value) => value * scale;
  public static Vector2D operator /(Vector2D value, double scale) => value * (1.0 / scale);
  public static Vector2D operator *(Vector2D first, Vector2D second) => new(first.X * second.X, first.Y * second.Y);
  public static Vector2D operator /(Vector2D first, Vector2D second) => new(first.X / second.X, first.Y / second.Y);
  public static explicit operator Vector2D(Point value) => new(value.X, value.Y);
  public static bool operator ==(Vector2D first, Vector2D second) => first.Equals(second);
  public static bool operator !=(Vector2D first, Vector2D second) => !first.Equals(second);
}
