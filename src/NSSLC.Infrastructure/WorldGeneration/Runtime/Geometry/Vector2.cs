using System;

namespace NSSLC.WorldGeneration.Geometry;

public struct Vector2 : IEquatable<Vector2> {
  public float X;
  public float Y;
  public static Vector2 Zero => new(0, 0);
  public static Vector2 One => new(1, 1);
  public static Vector2 UnitX => new(1, 0);
  public static Vector2 UnitY => new(0, 1);

  public Vector2(float x, float y) {
    X = x;
    Y = y;
  }
  public Vector2(float value) : this(value, value) { }
  public float LengthSquared() => X * X + Y * Y;
  public float Length() => (float)Math.Sqrt(LengthSquared());
  public void Normalize() {
    float scale = 1f / Length();
    X *= scale;
    Y *= scale;
  }
  public static Vector2 Normalize(Vector2 value) {
    value.Normalize();
    return value;
  }
  public static float Distance(Vector2 first, Vector2 second) => (first - second).Length();
  public static float DistanceSquared(Vector2 first, Vector2 second) =>
    (first - second).LengthSquared();
  public static float Dot(Vector2 first, Vector2 second) => first.X * second.X + first.Y * second.Y;
  public static Vector2 Lerp(Vector2 first, Vector2 second, float amount) =>
    first + (second - first) * amount;
  public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
  public override bool Equals(object obj) => obj is Vector2 other && Equals(other);
  public override int GetHashCode() => HashCode.Combine(X, Y);
  public override string ToString() => $"{{X:{X} Y:{Y}}}";
  public static Vector2 operator +(Vector2 first, Vector2 second) => new(first.X + second.X, first.Y + second.Y);
  public static Vector2 operator -(Vector2 first, Vector2 second) => new(first.X - second.X, first.Y - second.Y);
  public static Vector2 operator -(Vector2 value) => new(-value.X, -value.Y);
  public static Vector2 operator *(Vector2 value, float scale) => new(value.X * scale, value.Y * scale);
  public static Vector2 operator *(float scale, Vector2 value) => value * scale;
  public static Vector2 operator *(Vector2 first, Vector2 second) => new(first.X * second.X, first.Y * second.Y);
  public static Vector2 operator /(Vector2 value, float scale) => value * (1f / scale);
  public static Vector2 operator /(Vector2 first, Vector2 second) => new(first.X / second.X, first.Y / second.Y);
  public static bool operator ==(Vector2 first, Vector2 second) => first.Equals(second);
  public static bool operator !=(Vector2 first, Vector2 second) => !first.Equals(second);
}
