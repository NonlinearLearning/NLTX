namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct WorldSaveCommand
{
  public WorldSaveCommand(
    string path,
    bool isCloudSave,
    WorldBackupPolicy backupPolicy)
  {
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A world save path is required.", nameof(path))
      : path;
    IsCloudSave = isCloudSave;
    BackupPolicy = backupPolicy;
  }

  public string Path { get; }

  public bool IsCloudSave { get; }

  public WorldBackupPolicy BackupPolicy { get; }
}
