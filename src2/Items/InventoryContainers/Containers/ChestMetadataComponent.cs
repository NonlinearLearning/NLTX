namespace Terraria.Items.InventoryContainers;

public sealed class ChestMetadataComponent
{
  public ChestMetadataComponent(int x, int y, int index, int capacity, bool bankChest, string? name)
  {
    if (!ChestCapacityPolicy.IsValid(capacity))
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    if (!ChestMetadataPolicy.IsValidName(name))
    {
      throw new ArgumentException("The chest name exceeds the compatibility limit.", nameof(name));
    }

    X = x;
    Y = y;
    Index = index;
    Capacity = capacity;
    BankChest = bankChest;
    Name = name;
  }

  public int X { get; }

  public int Y { get; }

  public int Index { get; }

  public int Capacity { get; }

  public bool BankChest { get; private set; }

  public string? Name { get; private set; }

  public bool TryRename(string? name)
  {
    if (!ChestMetadataPolicy.IsValidName(name))
    {
      return false;
    }

    Name = name;
    return true;
  }

  public void SetBankClassification(bool bankChest)
  {
    BankChest = bankChest;
  }
}
