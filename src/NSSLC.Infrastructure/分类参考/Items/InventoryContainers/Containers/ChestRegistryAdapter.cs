namespace Terraria.Items.InventoryContainers;

public sealed class ChestRegistryAdapter
{
  private readonly Dictionary<InventoryPosition, int> _indicesByPosition = new();

  public bool TryAssign(InventoryPosition position, int index)
  {
    if (index < 0 || !_indicesByPosition.TryAdd(position, index))
    {
      return false;
    }

    return true;
  }

  public bool TryFind(InventoryPosition position, out int index)
  {
    return _indicesByPosition.TryGetValue(position, out index);
  }

  public bool Remove(InventoryPosition position)
  {
    return _indicesByPosition.Remove(position);
  }
}
