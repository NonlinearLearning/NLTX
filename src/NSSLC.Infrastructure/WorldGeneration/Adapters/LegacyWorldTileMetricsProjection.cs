using NSSLC.WorldGeneration;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>Projects world-scoped tile metrics into legacy counters and network updates.</summary>
internal static class LegacyWorldTileMetricsProjection
{
  internal static void PublishCompletedWindow(
    WorldTileMetricsSnapshot snapshot,
    bool sendNetworkUpdate)
  {
    WorldGen.totalEvil = snapshot.EvilCount;
    WorldGen.totalBlood = snapshot.BloodCount;
    WorldGen.totalSolid = snapshot.SolidCount;
    WorldGen.totalGood = snapshot.GoodCount;
    WorldGen.tGood = snapshot.GoodPercent;
    WorldGen.tEvil = snapshot.EvilPercent;
    WorldGen.tBlood = snapshot.BloodPercent;
    if (sendNetworkUpdate)
    {
      // Legacy CountTiles invokes message 57 whenever the X=0 publication runs.
      NetMessage.SendData(57);
    }
  }

  internal static void PublishPendingWindow(WorldTileMetricsSnapshot snapshot)
  {
    WorldGen.totalGood2 = snapshot.GoodCount;
    WorldGen.totalEvil2 = snapshot.EvilCount;
    WorldGen.totalBlood2 = snapshot.BloodCount;
    WorldGen.totalSolid2 = snapshot.SolidCount;
  }

  internal static void Reset()
  {
    WorldGen.totalGood = 0;
    WorldGen.totalEvil = 0;
    WorldGen.totalBlood = 0;
    WorldGen.totalSolid = 0;
    WorldGen.totalGood2 = 0;
    WorldGen.totalEvil2 = 0;
    WorldGen.totalBlood2 = 0;
    WorldGen.totalSolid2 = 0;
    WorldGen.tGood = 0;
    WorldGen.tEvil = 0;
    WorldGen.tBlood = 0;
    WorldGen.totalX = 0;
    WorldGen.totalD = 0;
  }
}
