using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Share.Attributes;

/// <summary>Immutable content definition used by the shared Buff system.</summary>
public sealed class Buff
{
  public Buff(
      BuffEffectId effect,
      BuffId immunityKey,
      int durationTicks,
      int maximumStacks = 1,
      IEnumerable<AttributeContribution>? contributions = null,
      IEnumerable<BuffBehaviorContribution>? behaviors = null,
      IEnumerable<BuffPeriodicEffect>? periodicEffects = null)
  {
    if (string.IsNullOrWhiteSpace(effect.Value))
    {
      throw new ArgumentException("A Buff definition requires an effect identifier.", nameof(effect));
    }

    if (string.IsNullOrWhiteSpace(immunityKey.Value))
    {
      throw new ArgumentException(
          "A Buff definition requires an immunity identifier.", nameof(immunityKey));
    }

    if (durationTicks == 0 || durationTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    if (maximumStacks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumStacks));
    }

    Effect = effect;
    ImmunityKey = immunityKey;
    DurationTicks = durationTicks;
    MaximumStacks = maximumStacks;
    Modifiers = Array.AsReadOnly(
        (contributions ?? Array.Empty<AttributeContribution>()).ToArray());
    Behaviors = Array.AsReadOnly(
        (behaviors ?? Array.Empty<BuffBehaviorContribution>()).ToArray());
    PeriodicEffects = Array.AsReadOnly(
        (periodicEffects ?? Array.Empty<BuffPeriodicEffect>()).ToArray());
  }

  public BuffEffectId Effect { get; }

  public BuffId ImmunityKey { get; }

  public int DurationTicks { get; }

  public int MaximumStacks { get; }

  public IReadOnlyList<AttributeContribution> Modifiers { get; }

  public IReadOnlyList<BuffBehaviorContribution> Behaviors { get; }

  public IReadOnlyList<BuffPeriodicEffect> PeriodicEffects { get; }
}
