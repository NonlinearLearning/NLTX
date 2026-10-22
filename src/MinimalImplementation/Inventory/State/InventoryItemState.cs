namespace MinimalImplementation.Server.Inventory.State;

public sealed record InventoryItemState(int Type, int Stack, byte Prefix, bool Favorited)
{
  public static InventoryItemState Empty { get; } = new(0, 0, 0, false);

  public bool IsEmpty => Type == 0 || Stack <= 0;

  public InventoryItemState WithStack(int stack)
  {
    return stack <= 0 ? Empty : this with { Stack = stack };
  }
}
