using System;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Runs the shared request buffer at the explicit Combat commit boundary.</summary>
public sealed class CombatDamageRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly CombatDamageRequestBuffer _buffer;
  private readonly CombatDamageSystem _combat;

  public CombatDamageRequestSystem(
      ArchWorld world,
      CombatDamageRequestBuffer buffer,
      IEntityRuntimeResolver resolver)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
    _combat = new CombatDamageSystem(World, resolver ?? throw new ArgumentNullException(nameof(resolver)));
  }

  public override void Update(in EcsServerTickContext context)
  {
    _buffer.BeginResultTick(context.Tick);
    IReadOnlyList<DamageRequest> requests = _buffer.DrainPending();
    IReadOnlyList<DamageProcessResult> results = _combat.Process(requests, context.Tick);
    for (int index = 0; index < requests.Count; index++)
    {
      DamageRequest request = requests[index];
      DamageProcessResult result = results[index];
      _buffer.Publish(in request, in result);
    }
  }
}
