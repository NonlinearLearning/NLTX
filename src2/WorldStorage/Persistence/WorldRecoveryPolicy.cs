namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldRecoveryPolicy
{
  public WorldRecoveryPolicy(int normalAttempts, int backupAttempts)
  {
    if (normalAttempts < 1 || normalAttempts > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(normalAttempts), "Normal attempts must be between 1 and 3.");
    }

    if (backupAttempts < 0 || backupAttempts > 2)
    {
      throw new ArgumentOutOfRangeException(nameof(backupAttempts), "Backup attempts must be between 0 and 2.");
    }

    NormalAttempts = normalAttempts;
    BackupAttempts = backupAttempts;
  }

  public int NormalAttempts { get; }

  public int BackupAttempts { get; }
}
