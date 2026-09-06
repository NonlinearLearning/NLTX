using System;

namespace Terraria.WorldSession.Calendar;

public sealed class Dd2RunStateComponent
{
  public const int DefaultLaneSpawnRate = 60;

  public Dd2RunStateComponent(
    bool ongoing = false,
    bool lostThisRun = false,
    bool wonThisRun = false,
    int ongoingDifficulty = 0,
    int laneSpawnRate = DefaultLaneSpawnRate,
    WorldTileRectangle arenaHitbox = default)
  {
    Ongoing = ongoing;
    LostThisRun = lostThisRun;
    WonThisRun = wonThisRun;
    OngoingDifficulty = ongoingDifficulty;
    LaneSpawnRate = laneSpawnRate;
    ArenaHitbox = arenaHitbox;
    Validate();
  }

  public bool Ongoing;
  public bool LostThisRun;
  public bool WonThisRun;
  public int OngoingDifficulty;
  public int LaneSpawnRate;
  public WorldTileRectangle ArenaHitbox;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(OngoingDifficulty);
    ArgumentOutOfRangeException.ThrowIfNegative(LaneSpawnRate);

    if (LostThisRun && WonThisRun)
    {
      throw new ArgumentException(
        "A DD2 run cannot be both won and lost.",
        nameof(WonThisRun));
    }

    ArenaHitbox.Validate();
  }
}
