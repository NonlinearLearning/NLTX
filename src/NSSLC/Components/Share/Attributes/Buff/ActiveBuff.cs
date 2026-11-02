using System;

namespace Terraria.Attributes;

/// <summary>One active Buff instance owned by the shared Buff list.</summary>
public readonly record struct ActiveBuff
{
  public ActiveBuff(
      BuffEffectId effect,
      BuffId immunityKey,
      long sourceId,
      int remainingTicks,
      int stacks)
  {
    if (string.IsNullOrWhiteSpace(effect.Value))
    {
      throw new ArgumentException("An active Buff requires an effect identifier.", nameof(effect));
    }

    if (string.IsNullOrWhiteSpace(immunityKey.Value))
    {
      throw new ArgumentException(
          "An active Buff requires an immunity identifier.", nameof(immunityKey));
    }

    if (sourceId == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceId));
    }

    if (remainingTicks == 0 || remainingTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    if (stacks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stacks));
    }

    Effect = effect;
    ImmunityKey = immunityKey;
    SourceId = sourceId;
    RemainingTicks = remainingTicks;
    Stacks = stacks;
  }

  public BuffEffectId Effect { get; }

  public BuffId ImmunityKey { get; }

  public long SourceId { get; }

  public int RemainingTicks { get; }

  public int Stacks { get; }

  public bool IsPermanent => RemainingTicks < 0;

  public ActiveBuff WithDuration(int remainingTicks)
  {
    return new ActiveBuff(Effect, ImmunityKey, SourceId, remainingTicks, Stacks);
  }

  public ActiveBuff WithStacks(int stacks)
  {
    return new ActiveBuff(Effect, ImmunityKey, SourceId, RemainingTicks, stacks);
  }
}
