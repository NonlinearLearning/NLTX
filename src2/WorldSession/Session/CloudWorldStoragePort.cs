namespace Terraria.WorldSession.Session;

public sealed class CloudWorldStoragePort
{
  public StoragePathValue? Path { get; private set; }

  public bool IsAvailable { get; private set; }

  public void Configure(StoragePathValue path, bool isAvailable)
  {
    Path = path;
    IsAvailable = isAvailable;
  }
}
