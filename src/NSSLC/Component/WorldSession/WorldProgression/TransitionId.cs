namespace Terraria.WorldProgression.Components;

public readonly record struct TransitionId(long Value)
{
  public static TransitionId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
