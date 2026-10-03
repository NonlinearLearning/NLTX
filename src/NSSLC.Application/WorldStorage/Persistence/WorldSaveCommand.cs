namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldSaveCommand
{
  public WorldSaveCommand(
    string path,
    WorldPersistenceDocument document,
    WorldBackupPolicy backupPolicy)
  {
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A world save path is required.", nameof(path))
      : path;
    Document = document ?? throw new ArgumentNullException(nameof(document));
    BackupPolicy = backupPolicy;
  }

  public string Path { get; }

  public WorldPersistenceDocument Document { get; }

  public WorldBackupPolicy BackupPolicy { get; }
}
