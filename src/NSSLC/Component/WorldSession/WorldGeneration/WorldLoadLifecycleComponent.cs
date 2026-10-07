using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界加载或生成的状态、失败恢复和重置要求。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：isGeneratingOrLoadingWorld（第 4151 行）； loadFailed（第 4165 行）； worldCleared（第 4167 行）；
/// worldBackup（第 4169 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 161 行。</para>
/// </remarks>
public sealed class WorldLoadLifecycleComponent
{
  public bool IsFresh =>
    RecoveryPhase == WorldLoadRecoveryPhase.NotStarted &&
    LoadAttemptCount == 0 &&
    !IsGeneratingOrLoadingWorld &&
    !LoadFailed &&
    !LoadCanceled &&
    !WorldCleared &&
    !RequiresWorldReset &&
    !WorldBackup;

  public WorldLoadRecoveryPhase RecoveryPhase { get; private set; }

  public int LoadAttemptCount { get; private set; }

  public bool IsGeneratingOrLoadingWorld { get; private set; }

  public bool LoadFailed { get; private set; }

  public bool LoadCanceled { get; private set; }

  public bool WorldCleared { get; private set; }

  public bool RequiresWorldReset { get; private set; }

  public bool WorldBackup { get; private set; }

  public bool PreservePreexistingLoadGate { get; private set; }

  internal void SetLoadingOrGenerating(bool isGeneratingOrLoadingWorld)
  {
    IsGeneratingOrLoadingWorld = isGeneratingOrLoadingWorld;
  }

  internal void SetLoadResult(bool loadFailed, bool worldBackup)
  {
    LoadFailed = loadFailed;
    LoadCanceled = false;
    RequiresWorldReset = false;
    WorldBackup = worldBackup;
  }

  internal void SetLoadCanceled(bool worldBackup)
  {
    LoadFailed = true;
    LoadCanceled = true;
    RequiresWorldReset = false;
    WorldBackup = worldBackup;
  }

  internal void MarkWorldCleared()
  {
    WorldCleared = true;
    RequiresWorldReset = false;
  }

  internal void ResetWorldCleared()
  {
    WorldCleared = false;
  }

  internal void BeginRecovery(bool preservePreexistingLoadGate)
  {
    RecoveryPhase = WorldLoadRecoveryPhase.PrimaryLoad;
    LoadAttemptCount = 1;
    RequiresWorldReset = false;
    WorldCleared = false;
    PreservePreexistingLoadGate = preservePreexistingLoadGate;
  }

  internal void SetLoadRequiresWorldReset(bool worldBackup)
  {
    LoadFailed = true;
    RequiresWorldReset = true;
    WorldBackup = worldBackup;
  }

  internal void MarkUnpublishedSessionDiscarded()
  {
    RequiresWorldReset = false;
  }

  internal void ContinueRecovery(
    WorldLoadRecoveryPhase phase,
    int loadAttemptCount)
  {
    RecoveryPhase = phase;
    LoadAttemptCount = loadAttemptCount;
  }
}
