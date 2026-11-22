using System;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>
/// Server-side Player attack submission boundary. It carries no client input or network state.
/// </summary>
public sealed class PlayerAttackIntentAdapter
{
  private readonly ArchWorld _world;

  public PlayerAttackIntentAdapter(ArchWorld world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public long Submit(
      EntityRuntimeHandle source,
      EntityRuntimeHandle target,
      CombatAttackDefinitionId definitionId,
      long tick,
      DamageContact? contact = null)
  {
    return CombatAttackIntentQueue.Require(_world).Enqueue(
        source,
        target,
        definitionId,
        tick,
        DamageCause.PlayerAttack,
        contact);
  }
}
