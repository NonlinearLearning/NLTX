namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneMetricsAggregateComponent
{
  private readonly int[] _tileCounts = new int[4];
  private readonly int[] _liquidCounts = new int[4];
  private readonly int[] _aggregateCounts =
    new int[Enum.GetValues<SceneMetricKind>().Length];

  public uint ScanRevision { get; private set; }

  public uint LastScanTime { get; private set; }

  public SceneScanRectangle TileCenter { get; private set; }

  public int BestOreType { get; private set; } = -1;

  public SceneScanRectangle BestOrePosition { get; private set; }

  public Guid? PerspectiveEntityId { get; private set; }

  public bool HasBanner { get; private set; }

  public bool CanPlayCreditsRoll { get; private set; }

  public Guid? ClosestNpcEntityId { get; private set; }

  public int ShimmerTileCount => GetCount(SceneMetricKind.ShimmerTile);

  public int EvilTileCount => GetCount(SceneMetricKind.EvilTile);

  public int HolyTileCount => GetCount(SceneMetricKind.HolyTile);

  public int HoneyBlockCount => GetCount(SceneMetricKind.HoneyBlock);

  public int SnowTileCount => GetCount(SceneMetricKind.SnowTile);

  public int TownNpcCount => GetCount(SceneMetricKind.TownNpc);

  public void Reset(
    uint scanRevision,
    uint lastScanTime,
    SceneScanRectangle tileCenter,
    Guid? perspectiveEntityId)
  {
    Array.Clear(_tileCounts);
    Array.Clear(_liquidCounts);
    Array.Clear(_aggregateCounts);
    ScanRevision = scanRevision;
    LastScanTime = lastScanTime;
    TileCenter = tileCenter;
    BestOreType = -1;
    BestOrePosition = default;
    PerspectiveEntityId = perspectiveEntityId;
    HasBanner = false;
    CanPlayCreditsRoll = false;
    ClosestNpcEntityId = null;
  }

  public void AddTile(int tileIndex, int amount = 1)
  {
    EnsureCountIndex(tileIndex, _tileCounts.Length);
    EnsureNonNegative(amount);
    _tileCounts[tileIndex] += amount;
  }

  public void AddLiquid(int liquidIndex, int amount = 1)
  {
    EnsureCountIndex(liquidIndex, _liquidCounts.Length);
    EnsureNonNegative(amount);
    _liquidCounts[liquidIndex] += amount;
  }

  public void AddMetric(SceneMetricKind kind, int amount = 1)
  {
    EnsureNonNegative(amount);
    _aggregateCounts[(int)kind] += amount;
  }

  public void SetBestOre(int type, SceneScanRectangle position)
  {
    BestOreType = type;
    BestOrePosition = position;
  }

  public void SetVisualFlags(bool hasBanner, bool canPlayCreditsRoll)
  {
    HasBanner = hasBanner;
    CanPlayCreditsRoll = canPlayCreditsRoll;
  }

  public void SetClosestNpc(Guid? entityId)
  {
    ClosestNpcEntityId = entityId;
  }

  public SceneMetricsAggregateValue Snapshot()
  {
    return new SceneMetricsAggregateValue(
      ScanRevision,
      LastScanTime,
      TileCenter,
      BestOreType,
      BestOrePosition,
      PerspectiveEntityId,
      (int[])_tileCounts.Clone(),
      (int[])_liquidCounts.Clone(),
      (int[])_aggregateCounts.Clone(),
      HasBanner,
      CanPlayCreditsRoll,
      ClosestNpcEntityId);
  }

  private int GetCount(SceneMetricKind kind)
  {
    return _aggregateCounts[(int)kind];
  }

  private static void EnsureCountIndex(int index, int length)
  {
    if ((uint)index >= (uint)length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }
  }

  private static void EnsureNonNegative(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }
  }
}
