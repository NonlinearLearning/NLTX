namespace MinimalImplementation.Server.Identity;

public readonly record struct EntityGeneration(long Value)
{
  public bool IsValid => Value > 0;
}
