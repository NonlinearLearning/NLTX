namespace Terraria.WorldSession.Runtime;

public sealed class RuntimeHostAdapter
{
  public bool AssetsAvailable { get; private set; }

  public void SetAssetsAvailable(bool available)
  {
    AssetsAvailable = available;
  }
}
