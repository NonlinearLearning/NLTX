using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存本次撒旦军队事件的难度、胜负和竞技场状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.DD2Event。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs。</para>
/// <para>
/// 主要源成员：LostThisRun（第 51 行）； WonThisRun（第 53 行）； LaneSpawnRate（第 55 行）； Ongoing（第 57 行）；
/// ArenaHitbox（第 61 行）； OngoingDifficulty（第 65 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 108 行。</para>
/// </remarks>
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
