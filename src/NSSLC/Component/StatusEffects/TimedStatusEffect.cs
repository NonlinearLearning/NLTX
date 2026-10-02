using Terraria.Relationships;

namespace Terraria.StatusEffects;

public readonly record struct TimedStatusEffect
{
  public TimedStatusEffect(
    int effectType,
    int remainingTicks,
    int stacks,
    EntityReference source)
  {
    EffectType = effectType;
    RemainingTicks = remainingTicks;
    Stacks = stacks;
    Source = source;
  }

  public int EffectType { get; }
  public int RemainingTicks { get; }
  public int Stacks { get; }
  public EntityReference Source { get; }
}
