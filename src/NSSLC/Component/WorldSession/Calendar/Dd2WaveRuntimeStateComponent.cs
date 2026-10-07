using System;
using System.Collections.Generic;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存撒旦军队当前波次、击杀、生成等待和水晶掉落状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.DD2Event。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs。</para>
/// <para>
/// 主要源成员：_crystalsDropping_lastWave（第 69 行）； _crystalsDropping_toDrop（第 71 行）；
/// _crystalsDropping_alreadyDropped（第 73 行）； _timeLeftUntilSpawningBegins（第 75 行）；
/// EnemySpawningIsOnHold（第 91 行）。
/// </para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：totalInvasionPoints（第 5937 行）； waveKills（第 5939 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 108 行。</para>
/// </remarks>
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
