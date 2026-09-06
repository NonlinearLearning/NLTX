using System;
using System.Collections.Generic;

namespace Terraria.WorldSession.Calendar;

public sealed class Dd2WaveRuntimeStateComponent
{
  private readonly IReadOnlyList<WorldTileCoordinate> _deadGoblinPositions;

  public Dd2WaveRuntimeStateComponent(
    int currentWave = 0,
    float waveKills = 0.0f,
    float totalInvasionPoints = 0.0f,
    int timeLeftUntilSpawningBegins = 0,
    IReadOnlyList<WorldTileCoordinate>? deadGoblinPositions = null,
    int crystalsDroppingLastWave = 0,
    int crystalsDroppingToDrop = 0,
    int crystalsDroppingAlreadyDropped = 0)
  {
    CurrentWave = currentWave;
    WaveKills = waveKills;
    TotalInvasionPoints = totalInvasionPoints;
    TimeLeftUntilSpawningBegins = timeLeftUntilSpawningBegins;
    _deadGoblinPositions = new List<WorldTileCoordinate>(
      deadGoblinPositions ?? Array.Empty<WorldTileCoordinate>()).AsReadOnly();
    CrystalsDroppingLastWave = crystalsDroppingLastWave;
    CrystalsDroppingToDrop = crystalsDroppingToDrop;
    CrystalsDroppingAlreadyDropped = crystalsDroppingAlreadyDropped;
    Validate();
  }

  public int CurrentWave;
  public float WaveKills;
  public float TotalInvasionPoints;
  public int TimeLeftUntilSpawningBegins;
  public IReadOnlyList<WorldTileCoordinate> DeadGoblinPositions =>
    _deadGoblinPositions;
  public int CrystalsDroppingLastWave;
  public int CrystalsDroppingToDrop;
  public int CrystalsDroppingAlreadyDropped;

  public bool EnemySpawningIsOnHold => TimeLeftUntilSpawningBegins != 0;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(CurrentWave);
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeftUntilSpawningBegins);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingLastWave);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingToDrop);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingAlreadyDropped);

    if (!float.IsFinite(WaveKills) || WaveKills < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(WaveKills));
    }

    if (!float.IsFinite(TotalInvasionPoints) || TotalInvasionPoints < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(TotalInvasionPoints));
    }

    if (CrystalsDroppingAlreadyDropped > CrystalsDroppingToDrop)
    {
      throw new ArgumentException(
        "Dropped crystals cannot exceed the pending crystal count.",
        nameof(CrystalsDroppingAlreadyDropped));
    }
  }
}
