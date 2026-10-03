namespace Terraria.NonAuthoritative.Persistence;

public enum WorldRecoveryStatus
{
  Loaded,
  RecoveredFromBackup,
  Missing,
  Canceled,
  Failed
}
