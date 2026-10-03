using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldLoadLifecycleComponent
{
  public WorldLoadRecoveryPhase RecoveryPhase { get; private set; }

  public int LoadAttemptCount { get; private set; }

  public bool IsGeneratingOrLoadingWorld { get; private set; }

  public bool LoadFailed { get; private set; }

  public bool LoadCanceled { get; private set; }

  public bool WorldCleared { get; private set; }

  public bool RequiresWorldReset { get; private set; }

  public bool WorldBackup { get; private set; }

  public void SetLoadingOrGenerating(bool isGeneratingOrLoadingWorld)
  {
    IsGeneratingOrLoadingWorld = isGeneratingOrLoadingWorld;
  }

  public void SetLoadResult(bool loadFailed, bool worldBackup)
  {
    LoadFailed = loadFailed;
    LoadCanceled = false;
    RequiresWorldReset = false;
    WorldBackup = worldBackup;
  }

  public void SetLoadCanceled(bool worldBackup)
  {
    LoadFailed = true;
    LoadCanceled = true;
    RequiresWorldReset = false;
    WorldBackup = worldBackup;
  }

  public void MarkWorldCleared()
  {
    WorldCleared = true;
    RequiresWorldReset = false;
  }

  public void ResetWorldCleared()
  {
    WorldCleared = false;
  }

  internal void BeginRecovery()
  {
    RecoveryPhase = WorldLoadRecoveryPhase.PrimaryLoad;
    LoadAttemptCount = 1;
    RequiresWorldReset = false;
    WorldCleared = false;
  }

  internal void SetLoadRequiresWorldReset(bool worldBackup)
  {
    LoadFailed = true;
    RequiresWorldReset = true;
    WorldBackup = worldBackup;
  }

  internal void ContinueRecovery(
    WorldLoadRecoveryPhase phase,
    int loadAttemptCount)
  {
    RecoveryPhase = phase;
    LoadAttemptCount = loadAttemptCount;
  }
}
