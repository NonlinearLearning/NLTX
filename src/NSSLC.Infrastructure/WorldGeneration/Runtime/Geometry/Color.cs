using System;

namespace NSSLC.WorldGeneration.Geometry;

public struct Color {
  public byte R;
  public byte G;
  public byte B;
  public byte A;
  public static Color White => new(255, 255, 255);
  public static Color Blue => new(0, 0, 255);
  public static Color Green => new(0, 128, 0);
  public static Color Pink => new(255, 192, 203);
  public static Color Transparent => new(0, 0, 0, 0);
  public Color(int red, int green, int blue, int alpha = 255) {
    R = (byte)Math.Clamp(red, 0, 255);
    G = (byte)Math.Clamp(green, 0, 255);
    B = (byte)Math.Clamp(blue, 0, 255);
    A = (byte)Math.Clamp(alpha, 0, 255);
  }
  public Color(float red, float green, float blue, float alpha = 1)
    : this((int)(red * 255), (int)(green * 255), (int)(blue * 255), (int)(alpha * 255)) { }
  public Color(Vector3 color) : this(color.X, color.Y, color.Z) { }
  public Color(Vector4 color) : this(color.X, color.Y, color.Z, color.W) { }
  public static Color Lerp(Color first, Color second, float amount) =>
    new((int)MathHelper.Lerp(first.R, second.R, amount), (int)MathHelper.Lerp(first.G, second.G, amount),
        (int)MathHelper.Lerp(first.B, second.B, amount), (int)MathHelper.Lerp(first.A, second.A, amount));
  public static Color operator *(Color value, float scale) =>
    new((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
}
