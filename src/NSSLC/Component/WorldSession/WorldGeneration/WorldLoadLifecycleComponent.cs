using System;

namespace Terraria.WorldGeneration.Components;

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
