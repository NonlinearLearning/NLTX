using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存待触发的世界事件及当日节日覆盖。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：forceHalloweenForToday（第 384 行）； afterPartyOfDoom（第 493 行）。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：spawnEye（第 4147 行）； spawnHardBoss（第 4149 行）； spawnMeteor（第 4163 行）； meteorShowerCount（第
/// 4271 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 1042 行。</para>
/// </remarks>
public sealed class PendingWorldEventStateComponent
{
  public PendingWorldEventStateComponent(
    bool spawnEye = false,
    int spawnHardBoss = 0,
    bool spawnMeteor = false,
    int meteorShowerCount = 0,
    bool afterPartyOfDoom = false,
    bool forceHalloweenForToday = false,
    bool forceChristmasForToday = false)
  {
    SpawnEye = spawnEye;
    SpawnHardBoss = spawnHardBoss;
    SpawnMeteor = spawnMeteor;
    MeteorShowerCount = meteorShowerCount;
    AfterPartyOfDoom = afterPartyOfDoom;
    ForceHalloweenForToday = forceHalloweenForToday;
    ForceChristmasForToday = forceChristmasForToday;
    Validate();
  }

  public bool SpawnEye;
  public int SpawnHardBoss;
  public bool SpawnMeteor;
  public int MeteorShowerCount;
  public bool AfterPartyOfDoom;
  public bool ForceHalloweenForToday;
  public bool ForceChristmasForToday;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SpawnHardBoss);
    ArgumentOutOfRangeException.ThrowIfNegative(MeteorShowerCount);
  }
}
