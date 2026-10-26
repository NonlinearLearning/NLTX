using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.StatusEffects;

/// <summary>Immutable content definition used by the shared Buff system.</summary>
public sealed class BuffDefinition
{
  public BuffDefinition(
      BuffEffectId effect,
      Terraria.Combat.BuffId immunityKey,
      int durationTicks,
      int maximumStacks = 1,
      IEnumerable<BuffAttributeModifierDefinition>? modifiers = null)
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
        (modifiers ?? Array.Empty<BuffAttributeModifierDefinition>()).ToArray());
  }

  public BuffEffectId Effect { get; }

  public Terraria.Combat.BuffId ImmunityKey { get; }

  public int DurationTicks { get; }

  public int MaximumStacks { get; }

  public IReadOnlyList<BuffAttributeModifierDefinition> Modifiers { get; }
}
