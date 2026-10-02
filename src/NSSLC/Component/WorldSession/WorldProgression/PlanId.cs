namespace Terraria.WorldProgression.Components;

public readonly record struct PlanId(long Value)
{
  public static PlanId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
