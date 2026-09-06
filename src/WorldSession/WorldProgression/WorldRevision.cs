namespace Terraria.WorldProgression.Components;

public readonly record struct WorldRevision(long Value)
{
  public static WorldRevision Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
