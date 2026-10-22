namespace MinimalImplementation.Server.Identity;

public readonly record struct WorldScopeId(long Value)
{
  public bool IsValid => Value > 0;
}
