using System.Numerics;

namespace NLTX.PlayerInputGameplay.Golf;

public sealed class GolfLocalTrackingComponent
{
  private readonly GolfBallTrackRecord[] _hitRecords;

  public GolfLocalTrackingComponent(int recordCapacity = 1000)
  {
    if (recordCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(recordCapacity));
    }

    _hitRecords = Enumerable.Range(0, recordCapacity)
      .Select(static _ => new GolfBallTrackRecord())
      .ToArray();
  }

  public int GolfScoreTime { get; private set; }

  public int GolfScoreTimeMax { get; private set; } = GolfRulesDefinition.ScoreTimeMax;

  public int GolfScoreDelay { get; private set; } = GolfRulesDefinition.ScoreDelay;

  public double? LastRecordedBallTime { get; private set; }

  public Vector2? LastRecordedBallLocation { get; private set; }

  public bool WaitingForBallToSettle { get; private set; }

  public int? LastHitGolfBallId { get; private set; }

  public int LastRecordedSwingCount { get; private set; }

  public IReadOnlyList<GolfBallTrackRecord> HitRecords => _hitRecords;

  public void ObserveBall(
    double timestamp,
    int projectileId,
    Vector2 location,
    int swingCount,
    bool isActive,
    bool isGolfBall,
    bool isLocallyOwned)
  {
    if (!isActive || !isGolfBall || !isLocallyOwned)
    {
      return;
    }

    LastRecordedBallTime = timestamp;
    LastRecordedBallLocation = location;
    LastHitGolfBallId = projectileId;
    LastRecordedSwingCount = swingCount;
    WaitingForBallToSettle = true;
  }

  public void Advance(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    GolfScoreTime = Math.Min(GolfScoreTime + ticks, GolfScoreTimeMax);
    if (WaitingForBallToSettle && ticks >= GolfScoreDelay)
    {
      WaitingForBallToSettle = false;
    }
  }

  public GolfBallTrackRecord GetRecord(int index)
  {
    if (index < 0 || index >= _hitRecords.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return _hitRecords[index];
  }

  public void Reset()
  {
    GolfScoreTime = 0;
    LastRecordedBallTime = null;
    LastRecordedBallLocation = null;
    LastHitGolfBallId = null;
    LastRecordedSwingCount = 0;
    WaitingForBallToSettle = false;
    foreach (var record in _hitRecords)
    {
      record.Clear();
    }
  }
}
