namespace Terraria.WorldGeneration.Components;

public enum WorldLoadRecoveryAction : byte
{
  LoadWorld,
  CheckBackup,
  RestoreBackupAndDelete,
  NotifyWorldLoaded,
  ReportNoBackupFailure,
  ReportLoadFailure,
}
