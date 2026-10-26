using System;

namespace Terraria.Combat;

/// <summary>
/// Immutable content rule used to create a one-tick damage payload. It is not runtime entity state.
/// </summary>
public sealed class CombatAttackDefinition
{
  public CombatAttackDefinition(CombatAttackDefinitionId id, DamagePayload payload)
  {
    if (!id.IsValid)
    {
      throw new ArgumentException("An attack definition id is required.", nameof(id));
    }

    if (!payload.IsValid)
    {
      throw new ArgumentException("A valid attack payload is required.", nameof(payload));
    }

    Id = id;
    Payload = payload;
  }

  public CombatAttackDefinitionId Id { get; }

  public DamagePayload Payload { get; }
}
