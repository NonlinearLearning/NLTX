namespace Terraria.Items.InventoryContainers;

public sealed class ItemDerivedValues
{
  public ItemDerivedValues(
    bool isActive,
    bool isAir,
    bool isCoin,
    string name,
    int originalRarity,
    int originalDamage,
    int originalDefense,
    int value)
  {
    IsActive = isActive;
    IsAir = isAir;
    IsCoin = isCoin;
    Name = name;
    OriginalRarity = originalRarity;
    OriginalDamage = originalDamage;
    OriginalDefense = originalDefense;
    Value = value;
  }

  public bool IsActive { get; }

  public bool IsAir { get; }

  public bool IsCoin { get; }

  public string Name { get; }

  public int OriginalRarity { get; }

  public int OriginalDamage { get; }

  public int OriginalDefense { get; }

  public int Value { get; }
}
