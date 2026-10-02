using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
  Assert(EqualityComparer<T>.Default.Equals(expected, actual),
    $"{message} Expected {expected}, got {actual}.");
}

WorldLoadLifecycleComponent load = new();
AssertEqual(
  WorldLoadRecoveryAction.LoadWorld,
  WorldLoadLifecycleSystem.StartRecovery(load),
  "Recovery begins with a primary load.");
AssertEqual(1, load.LoadAttemptCount, "The first load attempt is counted.");

WorldLoadLifecycleSystem.MarkWorldFileOpened(load);
Assert(!load.LoadFailed, "Opening the file clears the prior load failure flag.");
WorldLoadLifecycleSystem.BeginDecodedWorldRepair(load);
Assert(load.IsGeneratingOrLoadingWorld, "Decoded world repair raises the load gate.");
WorldLoadLifecycleSystem.CompleteLiquidSettle(load);
Assert(!load.IsGeneratingOrLoadingWorld, "Liquid settle completion clears the load gate.");

AssertEqual(
  WorldLoadRecoveryAction.LoadWorld,
  WorldLoadLifecycleSystem.CompleteLoadAttempt(load, loadFailed: true),
  "The first primary failure retries the primary file.");
AssertEqual(2, load.LoadAttemptCount, "The primary retry is counted.");
AssertEqual(
  WorldLoadRecoveryAction.CheckBackup,
  WorldLoadLifecycleSystem.CompleteLoadAttempt(load, loadFailed: true),
  "A second primary failure probes the backup.");
AssertEqual(
  WorldLoadRecoveryAction.ReportNoBackupFailure,
  WorldLoadLifecycleSystem.CompleteBackupCheck(load, backupExists: false),
  "Missing backup terminates recovery without another load.");
AssertEqual(WorldLoadRecoveryPhase.FailedNoBackup, load.RecoveryPhase, "No-backup terminal phase.");
Assert(load.LoadFailed && !load.WorldBackup, "No-backup result remains visible.");
AssertThrows<InvalidOperationException>(
  () => WorldLoadLifecycleSystem.CompleteLiquidSettle(load),
  "Liquid settle cannot clear a gate that is not raised.");
Console.WriteLine("PASS: primary retries and missing-backup terminal path");

WorldLoadLifecycleComponent backupLoad = new();
WorldLoadLifecycleSystem.StartRecovery(backupLoad);
WorldLoadLifecycleSystem.CompleteLoadAttempt(backupLoad, loadFailed: true);
AssertEqual(
  WorldLoadRecoveryAction.CheckBackup,
  WorldLoadLifecycleSystem.CompleteLoadAttempt(backupLoad, loadFailed: true),
  "Backup is only checked after the second primary failure.");
AssertEqual(
  WorldLoadRecoveryAction.RestoreBackupAndDelete,
  WorldLoadLifecycleSystem.CompleteBackupCheck(backupLoad, backupExists: true),
  "A found backup is copied over the primary and then deleted by the adapter.");
Assert(backupLoad.WorldBackup, "Backup presence is recorded.");
AssertEqual(
  WorldLoadRecoveryAction.LoadWorld,
  WorldLoadLifecycleSystem.CompleteBackupRestore(backupLoad),
  "A restored backup is loaded next.");
AssertEqual(3, backupLoad.LoadAttemptCount, "The first backup load is attempt three.");
AssertEqual(
  WorldLoadRecoveryAction.NotifyWorldLoaded,
  WorldLoadLifecycleSystem.CompleteLoadAttempt(backupLoad, loadFailed: false),
  "Successful backup load enables the completion effects.");
AssertEqual(WorldLoadRecoveryPhase.Completed, backupLoad.RecoveryPhase, "Successful terminal phase.");
Assert(!backupLoad.LoadFailed && backupLoad.WorldBackup, "Successful backup outcome is preserved.");
AssertThrows<InvalidOperationException>(
  () => WorldLoadLifecycleSystem.MarkWorldFileOpened(backupLoad),
  "A completed recovery cannot accept another file-open callback.");
Console.WriteLine("PASS: backup restore ordering and successful completion");

AssertEqual(
  WorldLoadRecoveryAction.LoadWorld,
  WorldLoadLifecycleSystem.StartRecovery(new WorldLoadLifecycleComponent()),
  "A separate lifecycle can run independently.");
AssertThrows<InvalidOperationException>(
  () => WorldLoadLifecycleSystem.StartRecovery(load),
  "A lifecycle cannot begin a second recovery run.");
Console.WriteLine("PASS: recovery state rejects an invalid restart");

static void AssertThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}
