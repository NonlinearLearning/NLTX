using System;

namespace NSSLC.WorldGeneration;

public static class GeometryExtensions {
  public static int ToInt(this bool value) => value ? 1 : 0;
  public static int ToDirectionInt(this bool value) => value ? 1 : -1;

  public static bool deepCompare<T>(this T[] left, T[] right) {
    if (left == null || right == null) {
      return ReferenceEquals(left, right);
    }
    return left.AsSpan().SequenceEqual(right);
  }

  public static bool deepCompare<T>(this T[,] left, T[,] right) {
    if (left == null || right == null) {
      return ReferenceEquals(left, right);
    }
    if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1)) {
      return false;
    }
    for (int x = 0; x < left.GetLength(0); x++) {
      for (int y = 0; y < left.GetLength(1); y++) {
        if (!Equals(left[x, y], right[x, y])) {
          return false;
        }
      }
    }
    return true;
  }
}
