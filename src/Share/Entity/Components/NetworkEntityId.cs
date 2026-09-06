namespace EntityEcs.Components;

public readonly record struct NetworkEntityId(int Value)
{
  public static NetworkEntityId None => new(-1);

  public bool IsAssigned => Value >= 0;
}
