namespace Terraria.WorldSession.Session;

public sealed class WorldStoragePathPort
{
  public StoragePathValue? Path { get; private set; }

  public void Configure(StoragePathValue path)
  {
    Path = path;
  }
}
