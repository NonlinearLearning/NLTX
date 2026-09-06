namespace Terraria.Items;

public readonly record struct NetworkId(long Value)
{
  public bool IsAssigned => Value != 0;
}
