using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Models the observed Version4 load retry order and exposes file operations as effects.
/// </summary>
public static class WorldLoadLifecycleSystem
{
  public static WorldLoadRecoveryAction StartRecovery(
    WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.NotStarted)
    {
      throw new InvalidOperationException(
        "World-load recovery can only start once for a lifecycle component.");
    }

    component.BeginRecovery();
    return WorldLoadRecoveryAction.LoadWorld;
  }

  public static void MarkWorldFileOpened(WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureLoadingAttempt(component.RecoveryPhase);
    component.SetLoadResult(false, component.WorldBackup);
  }

  public static void BeginDecodedWorldRepair(WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureLoadingAttempt(component.RecoveryPhase);
    component.SetLoadingOrGenerating(true);
  }

  public static void CompleteLiquidSettle(WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!component.IsGeneratingOrLoadingWorld)
    {
      throw new InvalidOperationException(
        "Liquid settle can only complete after the world load gate is raised.");
    }

    component.SetLoadingOrGenerating(false);
  }

  public static WorldLoadRecoveryAction CompleteLoadAttempt(
    WorldLoadLifecycleComponent component,
    bool loadFailed)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureLoadingAttempt(component.RecoveryPhase);
    component.SetLoadResult(loadFailed, component.WorldBackup);

    if (!loadFailed)
    {
      component.ContinueRecovery(WorldLoadRecoveryPhase.Completed, component.LoadAttemptCount);
      return WorldLoadRecoveryAction.NotifyWorldLoaded;
    }

    switch (component.RecoveryPhase)
    {
      case WorldLoadRecoveryPhase.PrimaryLoad:
        component.ContinueRecovery(WorldLoadRecoveryPhase.PrimaryRetry, 2);
        return WorldLoadRecoveryAction.LoadWorld;
      case WorldLoadRecoveryPhase.PrimaryRetry:
        component.ContinueRecovery(WorldLoadRecoveryPhase.BackupCheck, 2);
        return WorldLoadRecoveryAction.CheckBackup;
      case WorldLoadRecoveryPhase.BackupLoad:
        component.ContinueRecovery(WorldLoadRecoveryPhase.BackupRetry, 4);
        return WorldLoadRecoveryAction.LoadWorld;
      case WorldLoadRecoveryPhase.BackupRetry:
        component.ContinueRecovery(WorldLoadRecoveryPhase.Failed, component.LoadAttemptCount);
        return WorldLoadRecoveryAction.ReportLoadFailure;
      default:
        throw new InvalidOperationException(
          $"The current recovery phase {component.RecoveryPhase} is not a load attempt.");
    }
  }

  public static WorldLoadRecoveryAction CompleteBackupCheck(
    WorldLoadLifecycleComponent component,
    bool backupExists)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.BackupCheck)
    {
      throw new InvalidOperationException(
        "A backup can only be checked after both primary load attempts fail.");
    }

    component.SetLoadResult(true, backupExists);
    if (!backupExists)
    {
      component.ContinueRecovery(
        WorldLoadRecoveryPhase.FailedNoBackup,
        component.LoadAttemptCount);
      return WorldLoadRecoveryAction.ReportNoBackupFailure;
    }

    component.ContinueRecovery(
      WorldLoadRecoveryPhase.BackupRestore,
      component.LoadAttemptCount);
    return WorldLoadRecoveryAction.RestoreBackupAndDelete;
  }

  public static WorldLoadRecoveryAction CompleteBackupRestore(
    WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.BackupRestore)
    {
      throw new InvalidOperationException(
        "A backup must be found before it can replace the primary world file.");
    }

    component.ContinueRecovery(WorldLoadRecoveryPhase.BackupLoad, 3);
    return WorldLoadRecoveryAction.LoadWorld;
  }

  private static void EnsureLoadingAttempt(WorldLoadRecoveryPhase phase)
  {
    if (phase is not (
        WorldLoadRecoveryPhase.PrimaryLoad or
        WorldLoadRecoveryPhase.PrimaryRetry or
        WorldLoadRecoveryPhase.BackupLoad or
        WorldLoadRecoveryPhase.BackupRetry))
    {
      throw new InvalidOperationException(
        $"The current recovery phase {phase} is not a load attempt.");
    }
  }
}
