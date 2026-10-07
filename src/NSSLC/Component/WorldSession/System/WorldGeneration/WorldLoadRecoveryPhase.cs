namespace Terraria.WorldGeneration.Components;

public enum WorldLoadRecoveryPhase : byte
{
  NotStarted,
  PrimaryLoad,
  PrimaryRetry,
  BackupCheck,
  BackupRestore,
  BackupLoad,
  BackupRetry,
  Completed,
  FailedNoBackup,
  Failed,
}
