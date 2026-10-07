using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldTileMetricsComponent
{
  private int[] _tileCounts = Array.Empty<int>();

  internal int[] TileCounts => _tileCounts;

  internal int TileScanTickCount { get; set; }

  internal int NextTileColumnX { get; set; }

  internal int ScheduledTileColumnX { get; set; } = -1;

  private int _pendingGoodCount;
  private int _pendingEvilCount;
  private int _pendingBloodCount;
  private int _pendingSolidCount;
  private WorldTileMetricsSnapshot _publishedSnapshot;
  private bool _hasPublishedSnapshot;

  public WorldTileMetricsSnapshot CreateSnapshot()
  {
    return _publishedSnapshot;
  }

  public bool TryGetPublishedSnapshot(out WorldTileMetricsSnapshot snapshot)
  {
    snapshot = _publishedSnapshot;
    return _hasPublishedSnapshot;
  }

  internal WorldTileMetricsSnapshot CreatePendingSnapshot()
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

    return new WorldTileMetricsSnapshot(
      _pendingGoodCount,
      _pendingEvilCount,
      _pendingBloodCount,
      _pendingSolidCount,
      goodPercent,
      evilPercent,
      bloodPercent);
  }

  internal void Reset()
  {
    Array.Clear(_tileCounts, 0, _tileCounts.Length);
    TileScanTickCount = 0;
    NextTileColumnX = 0;
    ScheduledTileColumnX = -1;
    ClearPendingCounts();
    _publishedSnapshot = default;
    _hasPublishedSnapshot = false;
  }

  internal void EnsureTileTypeCount(int tileTypeCount)
  {
    if (tileTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileTypeCount));
    }

    if (_tileCounts.Length == tileTypeCount)
    {
      return;
    }

    _tileCounts = new int[tileTypeCount];
    Reset();
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
    _publishedSnapshot = CreatePendingSnapshot();
    _hasPublishedSnapshot = true;
  }

  private static byte GetPercent(int count, int solidCount)
  {
    if (solidCount == 0)
    {
      return 0;
    }

    return (byte)Math.Round((double)count / (double)solidCount * 100.0);
  }
}
