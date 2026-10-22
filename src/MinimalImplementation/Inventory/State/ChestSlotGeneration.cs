namespace MinimalImplementation.Server.Inventory.State;

public readonly record struct ChestSlotGeneration(long Value)
{
  public bool IsValid => Value > 0;
}
