namespace Terraria.Items.InventoryContainers;

public sealed class ChestAccessTrackerAdapter
{
  private readonly HashSet<int> _inUse = new();

  public bool TryOpen(int chestIndex)
  {
    return chestIndex >= 0 && _inUse.Add(chestIndex);
  }

  public bool Close(int chestIndex)
  {
    return _inUse.Remove(chestIndex);
  }

  public bool IsInUse(int chestIndex)
  {
    return _inUse.Contains(chestIndex);
  }
}
