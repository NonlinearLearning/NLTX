using System;
using System.Numerics;
using Arch.Core;

namespace Terraria.Dome.Simulation.Golf;

public struct GolfRoundStateComponent
{
  public const int MaximumHitRecords = 1000;

  public Entity? CurrentBall;
  public int Score;
  public int ScoreTime;
  public int ScoreTimeMaximum;
  public int ScoreDelay;
  public double LastRecordedBallTime;
  public Vector2? LastRecordedBallLocation;
  public bool WaitingForBallToSettle;
  public int LastRecordedSwingCount;
  public GolfBallTrackRecord[] HitRecords;

  public GolfRoundStateComponent(int scoreTimeMaximum = 3600, int scoreDelay = 90)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scoreTimeMaximum);
    ArgumentOutOfRangeException.ThrowIfNegative(scoreDelay);
    CurrentBall = null;
    Score = 0;
    ScoreTime = 0;
    ScoreTimeMaximum = scoreTimeMaximum;
    ScoreDelay = scoreDelay;
    LastRecordedBallTime = 0.0d;
    LastRecordedBallLocation = null;
    WaitingForBallToSettle = false;
    LastRecordedSwingCount = 0;
    HitRecords = new GolfBallTrackRecord[MaximumHitRecords];
  }
}
