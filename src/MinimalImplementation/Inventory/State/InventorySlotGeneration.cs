namespace MinimalImplementation.Server.Inventory.State;

public readonly record struct InventorySlotGeneration(long Value)
{
  public bool IsValid => Value > 0;

  public InventorySlotGeneration Next()
  {
    if (!IsValid || Value == long.MaxValue)
    {
      throw new InvalidOperationException("The inventory slot generation cannot advance.");
    }

    return new InventorySlotGeneration(Value + 1);
  }
}
