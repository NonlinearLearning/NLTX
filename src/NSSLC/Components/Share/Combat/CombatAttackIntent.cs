using System;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>One immutable source attack intent captured for the current Tick.</summary>
public readonly record struct CombatAttackIntent
{
  public CombatAttackIntent(
      EntityRuntimeHandle source,
      EntityRuntimeHandle target,
      CombatAttackDefinitionId definitionId,
      long tick,
      DamageCause cause,
      DamageContact? contact = null,
      long requestId = 0)
  {
    if (!source.IsValid || !target.IsValid || !definitionId.IsValid)
    {
      throw new ArgumentException("Source, target and attack definition are required.");
    }

    if (tick < 0 || requestId < 0 || !Enum.IsDefined(cause))
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    Source = source;
    Target = target;
    DefinitionId = definitionId;
    Tick = tick;
    Cause = cause;
    Contact = contact;
    RequestId = requestId;
  }

  public EntityRuntimeHandle Source { get; }

  public EntityRuntimeHandle Target { get; }

  public CombatAttackDefinitionId DefinitionId { get; }

  public long Tick { get; }

  public DamageCause Cause { get; }

  public DamageContact? Contact { get; }

  public long RequestId { get; }
}
