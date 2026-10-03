using System;
using System.Collections.Generic;

namespace NSSLC.WorldGeneration.GameContent.NetModules;

internal static class NetLiquidModule {
  public static void CreateAndBroadcastByChunk(HashSet<int> changes) {
    throw new NotSupportedException("World generation runs without a network host.");
  }
}
