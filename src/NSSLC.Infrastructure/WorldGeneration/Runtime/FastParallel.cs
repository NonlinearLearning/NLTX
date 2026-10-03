using System;

namespace NSSLC.WorldGeneration.Runtime;

internal static class FastParallel {
  public static void For(int start, int end, Action<int, int, object> body) {
    body.Invoke(start, end, null);
  }
}
