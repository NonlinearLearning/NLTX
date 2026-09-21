namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldBackupPolicy
{
  public WorldBackupPolicy(int backupsToKeep)
  {
    if (backupsToKeep < 0 || backupsToKeep > 32)
    {
      throw new ArgumentOutOfRangeException(nameof(backupsToKeep), "Backup count must be between 0 and 32.");
    }

    BackupsToKeep = backupsToKeep;
  }

  public int BackupsToKeep { get; }
}
