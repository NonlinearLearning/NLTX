using System;

namespace Terraria.Share.Attributes;

/// <summary>Cross-domain intent to apply one status effect to an entity.</summary>
public readonly record struct BuffApplyRequestComponent
{
  public BuffApplyRequestComponent(
      BuffEffectId effect,
      long sourceId,
      int requestedStacks = 1,
      int? durationTicks = null)
  {
    if (string.IsNullOrWhiteSpace(effect.Value))
    {
      throw new ArgumentException("A status effect is required.", nameof(effect));
    }

    if (sourceId == 0 || requestedStacks <= 0 || durationTicks == 0 || durationTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceId));
    }

    Effect = effect;
    SourceId = sourceId;
    RequestedStacks = requestedStacks;
    DurationTicks = durationTicks;
  }

  public BuffEffectId Effect { get; }

  public long SourceId { get; }

  public int RequestedStacks { get; }

  public int? DurationTicks { get; }
}
