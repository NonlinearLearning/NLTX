namespace Terraria.NonAuthoritative.Platform;

public interface ICloudFileStore
{
  bool Exists(string path);

  byte[] ReadAllBytes(string path);

  bool WriteAllBytes(string path, ReadOnlyMemory<byte> data);

  bool Copy(string sourcePath, string destinationPath);

  bool Move(string sourcePath, string destinationPath);

  bool Delete(string path);
}
