namespace Terraria.WorldSession.Session;

public sealed class PlayerFileSelectionAdapter
{
  public StoragePathValue? SelectedPath { get; private set; }

  public void Select(StoragePathValue path)
  {
    SelectedPath = path;
  }

  public void Clear()
  {
    SelectedPath = null;
  }
}
