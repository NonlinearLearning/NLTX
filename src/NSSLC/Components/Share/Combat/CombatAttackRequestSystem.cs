using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// Converts one tick's source-neutral attack intents into validated damage requests.
/// </summary>
/// <remarks>
/// This is the only boundary that resolves an attack definition and captures the target's current
/// Life snapshot. Producers never attach payloads or Life snapshots to their entities.
/// </remarks>
public sealed class CombatAttackRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly CombatAttackIntentBuffer _intents;
  private readonly CombatDamageRequestBuffer _requests;
  private readonly CombatAttackDefinitionCatalog _definitions;
  private readonly IEntityRuntimeResolver _resolver;

  public CombatAttackRequestSystem(
      ArchWorld world,
      CombatAttackIntentBuffer intents,
      CombatDamageRequestBuffer requests,
      CombatAttackDefinitionCatalog definitions,
      IEntityRuntimeResolver resolver)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _intents = intents ?? throw new ArgumentNullException(nameof(intents));
    _requests = requests ?? throw new ArgumentNullException(nameof(requests));
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public override void Update(in EcsServerTickContext context)
  {
    IReadOnlyList<CombatAttackIntent> intents = _intents.Drain(context.Tick);
    for (int index = 0; index < intents.Count; index++)
    {
      CombatAttackIntent intent = intents[index];
      if (!_definitions.TryGet(intent.DefinitionId, out CombatAttackDefinition? definition) ||
          !_resolver.TryResolve(intent.Source, out ArchEntity source) ||
          source == ArchEntity.Null || !World.IsAlive(source) ||
          !_resolver.TryResolve(intent.Target, out ArchEntity target) ||
          target == ArchEntity.Null || !World.IsAlive(target) ||
          !World.Has<LifeComponent>(target))
      {
        continue;
      }

      LifeComponent targetLife = World.Get<LifeComponent>(target);
      if (!targetLife.IsAlive)
      {
        continue;
      }

      DamageRequest request = new(
          intent.Source,
          intent.Target,
          definition.Payload,
          targetLife,
          intent.Tick,
          intent.Cause,
          intent.Contact,
          intent.RequestId);
      _requests.Enqueue(in request);
    }
  }
}
