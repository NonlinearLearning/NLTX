namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemEconomyPayload
{
  public WorldItemEconomyPayload(
    int type,
    int stack,
    bool favorited,
    int value,
    int maxStack,
    int rarity,
    string name,
    bool isCoin,
    bool isAir)
  {
    Type = type;
    Stack = stack;
    Favorited = favorited;
    Value = value;
    MaxStack = maxStack;
    Rarity = rarity;
    Name = name;
    IsCoin = isCoin;
    IsAir = isAir;
  }

  public int Type { get; }

  public int Stack { get; }

  public bool Favorited { get; }

  public int Value { get; }

  public int MaxStack { get; }

  public int Rarity { get; }

  public string Name { get; }

  public bool IsCoin { get; }

  public bool IsAir { get; }
}
