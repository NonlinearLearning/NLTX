namespace MinimalImplementation.Server.Inventory.State;

public readonly record struct ChestGeneration(long Value)
{
  public bool IsValid => Value > 0;
}
