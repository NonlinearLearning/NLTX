namespace Terraria.Items;

public readonly record struct ItemState(int Type, byte Prefix, int Stack)
{
  public bool IsEmpty => Type == 0 || Stack <= 0;
}
