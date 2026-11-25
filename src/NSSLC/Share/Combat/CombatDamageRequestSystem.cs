using System;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Runs the shared request buffer at the explicit Combat commit boundary.</summary>
public sealed class CombatDamageRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly CombatDamageSystem _combat;

  public CombatDamageRequestSystem(
      ArchWorld world,
      IEntityRuntimeResolver resolver)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _combat = new CombatDamageSystem(
        World,
        resolver ?? throw new ArgumentNullException(nameof(resolver)));
  }

  public override void Update(in EcsServerTickContext context)
  {
    CombatDamageResultQueueComponent resultQueue = CombatDamageResultQueue.Require(World);
    resultQueue.BeginResultTick(context.Tick);
    IReadOnlyList<DamageRequest> requests = CombatDamageRequestQueue.Require(World).DrainPending();
    IReadOnlyList<DamageProcessResult> results = _combat.Process(requests, context.Tick);
    for (int index = 0; index < requests.Count; index++)
    {
      DamageRequest request = requests[index];
      DamageProcessResult result = results[index];
      resultQueue.Publish(in request, in result);
    }
  }
}
