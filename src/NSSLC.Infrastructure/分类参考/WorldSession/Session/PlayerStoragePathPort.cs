namespace Terraria.WorldSession.Session;

public sealed class PlayerStoragePathPort
{
  public StoragePathValue? Path { get; private set; }

  public void Configure(StoragePathValue path)
  {
    Path = path;
  }
}
