namespace Terraria.StatusEffects;

public sealed class StatusEffectsComponent
{
  public StatusEffectsComponent(IReadOnlyList<TimedStatusEffect> effects)
  {
    Effects = new List<TimedStatusEffect>(effects);
  }

  public List<TimedStatusEffect> Effects;
}
