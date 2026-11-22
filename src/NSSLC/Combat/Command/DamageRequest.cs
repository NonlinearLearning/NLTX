using System;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Validated, immutable request to apply one damage payload to one entity.</summary>
public readonly record struct DamageRequest
{
  public DamageRequest(
      EntityRuntimeHandle source,
      EntityRuntimeHandle target,
      DamagePayload payload,
      LifeComponent expectedTargetLife,
      long tick,
      DamageCause cause,
      DamageContact? contact = null,
      long requestId = 0,
      long effectSourceId = 0)
  {
    if (!source.IsValid)
    {
      throw new ArgumentException("A damage source handle is required.", nameof(source));
    }

    if (!target.IsValid)
    {
      throw new ArgumentException("A damage target handle is required.", nameof(target));
    }

    if (!payload.IsValid)
    {
      throw new ArgumentException("A valid damage payload is required.", nameof(payload));
    }

    if (expectedTargetLife.Maximum <= 0)
    {
      throw new ArgumentException("An expected target life snapshot is required.",
          nameof(expectedTargetLife));
    }

    if (tick < 0 || requestId < 0 || effectSourceId < 0 || !Enum.IsDefined(cause))
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    Source = source;
    Target = target;
    Payload = payload;
    ExpectedTargetLife = expectedTargetLife;
    Tick = tick;
    Cause = cause;
    Contact = contact;
    RequestId = requestId;
    EffectSourceId = effectSourceId;
  }

  public EntityRuntimeHandle Source { get; }

  public EntityRuntimeHandle Target { get; }

  public DamagePayload Payload { get; }

  public LifeComponent ExpectedTargetLife { get; }

  public long Tick { get; }

  public DamageCause Cause { get; }

  public DamageContact? Contact { get; }

  /// <summary>Source-local identity that distinguishes separate requests in the same tick.</summary>
  public long RequestId { get; }

  /// <summary>Optional status-effect source identity preserved across periodic damage requests.</summary>
  public long EffectSourceId { get; }

  public DamageRequestKey Key => new(Source, Target, Tick, Cause, RequestId, EffectSourceId);
}

/// <summary>Stable key used to reject duplicate applications in one logical tick.</summary>
public readonly record struct DamageRequestKey(
    EntityRuntimeHandle Source,
    EntityRuntimeHandle Target,
    long Tick,
    DamageCause Cause,
    long RequestId,
    long EffectSourceId = 0);
