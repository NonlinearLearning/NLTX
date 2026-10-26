using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>Consumes every Player-owned Combat result without storing a LastResult component.</summary>
public sealed class PlayerCombatResultSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly CombatDamageRequestBuffer _results;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      PlayerDeathStateComponent>();

  public PlayerCombatResultSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles,
      CombatDamageRequestBuffer results)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _results = results ?? throw new ArgumentNullException(nameof(results));
  }

  public override void Update(in EcsServerTickContext context)
  {
    World.Query(
        in _query,
        (ArchEntity entity) =>
        {
          if (_handles.TryGetHandle(entity, out EntityRuntimeHandle source))
          {
            _ = _results.ConsumeResults(source);
          }
        });
  }
}
