using System;
using Terraria.Combat;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>
/// Server-side Player attack submission boundary. It carries no client input or network state.
/// </summary>
public sealed class PlayerAttackIntentAdapter
{
  private readonly CombatAttackIntentBuffer _intents;

  public PlayerAttackIntentAdapter(CombatAttackIntentBuffer intents)
  {
    _intents = intents ?? throw new ArgumentNullException(nameof(intents));
  }

  public long Submit(
      EntityRuntimeHandle source,
      EntityRuntimeHandle target,
      CombatAttackDefinitionId definitionId,
      long tick,
      DamageContact? contact = null)
  {
    return _intents.Enqueue(
        source,
        target,
        definitionId,
        tick,
        DamageCause.PlayerAttack,
        contact);
  }
}
