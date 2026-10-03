using System;

namespace NSSLC.WorldGeneration.Geometry;

public struct Rectangle : IEquatable<Rectangle> {
  public int X;
  public int Y;
  public int Width;
  public int Height;
  public int Left => X;
  public int Top => Y;
  public int Right => X + Width;
  public int Bottom => Y + Height;
  public Point Center => new(X + Width / 2, Y + Height / 2);
  public static Rectangle Empty => new(0, 0, 0, 0);

  public Rectangle(int x, int y, int width, int height) {
    X = x;
    Y = y;
    Width = width;
    Height = height;
  }
  public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;
  public bool Contains(Point point) => Contains(point.X, point.Y);
  public bool Contains(Rectangle other) => other.Left >= Left && other.Right <= Right &&
    other.Top >= Top && other.Bottom <= Bottom;
  public bool Intersects(Rectangle other) => other.Left < Right && Left < other.Right &&
    other.Top < Bottom && Top < other.Bottom;
  public void Inflate(int horizontal, int vertical) {
    X -= horizontal;
    Y -= vertical;
    Width += horizontal * 2;
    Height += vertical * 2;
  }
  public void Offset(int x, int y) {
    X += x;
    Y += y;
  }
  public void Offset(Point offset) => Offset(offset.X, offset.Y);
  public static Rectangle Intersect(Rectangle first, Rectangle second) {
    int left = Math.Max(first.Left, second.Left);
    int top = Math.Max(first.Top, second.Top);
    int right = Math.Min(first.Right, second.Right);
    int bottom = Math.Min(first.Bottom, second.Bottom);
    return right > left && bottom > top ? new(left, top, right - left, bottom - top) : Empty;
  }
  public static Rectangle Union(Rectangle first, Rectangle second) {
    int left = Math.Min(first.Left, second.Left);
    int top = Math.Min(first.Top, second.Top);
    return new(left, top, Math.Max(first.Right, second.Right) - left,
               Math.Max(first.Bottom, second.Bottom) - top);
  }
  public bool Equals(Rectangle other) => X == other.X && Y == other.Y &&
    Width == other.Width && Height == other.Height;
  public override bool Equals(object obj) => obj is Rectangle other && Equals(other);
  public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
  public static bool operator ==(Rectangle first, Rectangle second) => first.Equals(second);
  public static bool operator !=(Rectangle first, Rectangle second) => !first.Equals(second);
}
