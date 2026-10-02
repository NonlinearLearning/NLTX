using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldTileMetricsComponent
{
  internal int TileScanTickCount { get; set; }

  internal int NextTileColumnX { get; set; }

  private int _pendingGoodCount;
  private int _pendingEvilCount;
  private int _pendingBloodCount;
  private int _pendingSolidCount;
  private WorldTileMetricsSnapshot _publishedSnapshot;

  public WorldTileMetricsSnapshot CreateSnapshot()
  {
    return _publishedSnapshot;
  }

  internal void AddGoodCount(int count)
  {
    _pendingGoodCount += count;
  }

  internal void AddEvilCount(int count)
  {
    _pendingEvilCount += count;
  }

  internal void AddBloodCount(int count)
  {
    _pendingBloodCount += count;
  }

  internal void AddSolidCount(int count)
  {
    _pendingSolidCount += count;
  }

  internal void ClearPendingCounts()
  {
    _pendingGoodCount = 0;
    _pendingEvilCount = 0;
    _pendingBloodCount = 0;
    _pendingSolidCount = 0;
  }

  internal void PublishPendingCounts()
  {
    byte goodPercent = GetPercent(_pendingGoodCount, _pendingSolidCount);
    byte evilPercent = GetPercent(_pendingEvilCount, _pendingSolidCount);
    byte bloodPercent = GetPercent(_pendingBloodCount, _pendingSolidCount);

    if (goodPercent == 0 && _pendingGoodCount > 0)
    {
      goodPercent = 1;
    }

    if (evilPercent == 0 && _pendingEvilCount > 0)
    {
      evilPercent = 1;
    }

    if (bloodPercent == 0 && _pendingBloodCount > 0)
    {
      bloodPercent = 1;
    }

    _publishedSnapshot = new WorldTileMetricsSnapshot(
      _pendingGoodCount,
      _pendingEvilCount,
      _pendingBloodCount,
      _pendingSolidCount,
      goodPercent,
      evilPercent,
      bloodPercent);
  }

  private static byte GetPercent(int count, int solidCount)
  {
    return (byte)Math.Round((double)count / (double)solidCount * 100.0);
  }
}
