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

  /// <summary>
  /// Releases the load gate after a terminal failure that did not partially commit owner state.
  /// </summary>
  public static void ReleaseLoadGateAfterFailure(WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase is not (
        WorldLoadRecoveryPhase.Failed or
        WorldLoadRecoveryPhase.FailedNoBackup) || component.RequiresWorldReset)
    {
      throw new InvalidOperationException(
        "The load gate can only be released after a terminal failure without a reset requirement.");
    }

    component.SetLoadingOrGenerating(false);
  }

  /// <summary>
  /// Records completion of the host's world-level cleanup after partial owner commit.
  /// </summary>
  public static void CompleteWorldReset(WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.Failed ||
        !component.RequiresWorldReset)
    {
      throw new InvalidOperationException(
        "World reset can only complete after a failed attempt that requires world cleanup.");
    }

    component.MarkWorldCleared();
    component.SetLoadingOrGenerating(false);
  }

  public static WorldLoadRecoveryAction CompleteLoadAttempt(
    WorldLoadLifecycleComponent component,
    bool loadFailed)
  {
    return CompleteLoadAttempt(component, loadFailed, requiresWorldReset: false);
  }

  /// <summary>
  /// Completes a load attempt and prevents retry when owner state may be partially committed.
  /// </summary>
  public static WorldLoadRecoveryAction CompleteLoadAttempt(
    WorldLoadLifecycleComponent component,
    bool loadFailed,
    bool requiresWorldReset)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureLoadingAttempt(component.RecoveryPhase);
    if (requiresWorldReset && !loadFailed)
    {
      throw new ArgumentException(
        "A successful load attempt cannot require a world reset.",
        nameof(requiresWorldReset));
    }

    if (requiresWorldReset)
    {
      component.SetLoadRequiresWorldReset(component.WorldBackup);
      component.ContinueRecovery(WorldLoadRecoveryPhase.Failed, component.LoadAttemptCount);
      return WorldLoadRecoveryAction.ReportLoadFailure;
    }

    if (!loadFailed && component.IsGeneratingOrLoadingWorld)
    {
      throw new InvalidOperationException(
        "A world cannot be reported loaded while the load gate is raised.");
    }

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

  /// <summary>
  /// Terminates an active recovery attempt after cooperative cancellation without scheduling a
  /// primary or backup retry.
  /// </summary>
  public static WorldLoadRecoveryAction CancelRecovery(
    WorldLoadLifecycleComponent component)
  {
    return CancelRecovery(component, requiresWorldReset: false);
  }

  /// <summary>
  /// Terminates cancellation and records whether owner state requires world-level cleanup.
  /// </summary>
  public static WorldLoadRecoveryAction CancelRecovery(
    WorldLoadLifecycleComponent component,
    bool requiresWorldReset)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase is not (
        WorldLoadRecoveryPhase.PrimaryLoad or
        WorldLoadRecoveryPhase.PrimaryRetry or
        WorldLoadRecoveryPhase.BackupCheck or
        WorldLoadRecoveryPhase.BackupRestore or
        WorldLoadRecoveryPhase.BackupLoad or
        WorldLoadRecoveryPhase.BackupRetry))
    {
      throw new InvalidOperationException(
        $"The current recovery phase {component.RecoveryPhase} cannot be canceled.");
    }

    component.SetLoadCanceled(component.WorldBackup);
    if (requiresWorldReset)
    {
      component.SetLoadRequiresWorldReset(component.WorldBackup);
    }

    component.ContinueRecovery(WorldLoadRecoveryPhase.Failed, component.LoadAttemptCount);
    return WorldLoadRecoveryAction.ReportLoadFailure;
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

  public static WorldLoadRecoveryAction FailBackupCheck(
    WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.BackupCheck)
    {
      throw new InvalidOperationException(
        "A backup check can only fail after both primary load attempts fail.");
    }

    component.SetLoadResult(true, component.WorldBackup);
    component.ContinueRecovery(WorldLoadRecoveryPhase.Failed, component.LoadAttemptCount);
    return WorldLoadRecoveryAction.ReportLoadFailure;
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

  public static WorldLoadRecoveryAction FailBackupRestore(
    WorldLoadLifecycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.RecoveryPhase != WorldLoadRecoveryPhase.BackupRestore)
    {
      throw new InvalidOperationException(
        "A backup restore can only fail after a backup has been found.");
    }

    component.SetLoadResult(true, worldBackup: true);
    component.ContinueRecovery(WorldLoadRecoveryPhase.Failed, component.LoadAttemptCount);
    return WorldLoadRecoveryAction.ReportLoadFailure;
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
