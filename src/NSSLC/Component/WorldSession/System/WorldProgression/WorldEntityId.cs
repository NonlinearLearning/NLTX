namespace Terraria.WorldProgression.Components;

public readonly record struct WorldEntityId(long Value)
{
  public static WorldEntityId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
