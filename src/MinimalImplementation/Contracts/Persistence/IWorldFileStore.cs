namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldFileStore
{
  WorldStorageReadResult ReadAllBytes(string path);

  WorldStorageOperationResult WriteAllBytes(string path, ReadOnlyMemory<byte> data);

  WorldStorageOperationResult Delete(string path, bool forceDelete);

  WorldStorageOperationResult CheckBackupExists(
    string worldPath,
    out bool backupExists);

  WorldStorageOperationResult RestoreBackupAndDelete(string worldPath);
}
