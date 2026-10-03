using System;

namespace NSSLC.WorldGeneration.Geometry;

public static class MathHelper {
  public static float Clamp(float value, float minimum, float maximum) =>
    Math.Min(Math.Max(value, minimum), maximum);
  public static float Lerp(float first, float second, float amount) => first + (second - first) * amount;
  public static float Max(float first, float second) => Math.Max(first, second);
}
