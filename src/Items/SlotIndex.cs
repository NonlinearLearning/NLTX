namespace Terraria.Items;

public readonly record struct SlotIndex(int Value)
{
  public bool IsValid => Value >= 0;
}
