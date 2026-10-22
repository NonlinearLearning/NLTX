namespace MinimalImplementation.Server.Inventory.State;

public readonly record struct WorldItemGeneration(long Value)
{
  public bool IsValid => Value > 0;
}
