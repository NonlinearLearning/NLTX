namespace Terraria.Items.InventoryContainers;

public sealed class ItemDefinition
{
  public ItemDefinition(
    int type,
    string name,
    int rarity,
    int damage,
    int defense,
    int value,
    int maxStack)
  {
    if (type <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }

    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("An item definition requires a name.", nameof(name));
    }

    if (maxStack <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxStack));
    }

    Type = type;
    Name = name;
    Rarity = rarity;
    Damage = damage;
    Defense = defense;
    Value = value;
    MaxStack = maxStack;
  }

  public int Type { get; }

  public string Name { get; }

  public int Rarity { get; }

  public int Damage { get; }

  public int Defense { get; }

  public int Value { get; }

  public int MaxStack { get; }
}
