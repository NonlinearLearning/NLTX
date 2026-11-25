using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Movement;

/// <summary>
/// Advances and recalculates gravity projections for entities with complete gravity state.
/// </summary>
/// <remarks>
/// The system only updates the gravity modifier collection and derived state. Movement and
/// physics consume <see cref="GravityStateComponent"/> in their own phase.
/// </remarks>
public sealed class GravityTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  public GravityTickSystem(ArchWorld world) : base(world) { }

  public override void Update(in EcsServerTickContext context)
  {
    QueryDescription query = new QueryDescription()
        .WithAll<GravityConfigurationComponent>()
        .WithAll<GravityDirectionStateComponent>()
        .WithAll<GravityModifierCollectionComponent>()
        .WithAll<GravityStateComponent>();

    context.World.Query(
        in query,
        static (
            ref GravityConfigurationComponent configuration,
            ref GravityDirectionStateComponent direction,
            ref GravityModifierCollectionComponent modifiers,
            ref GravityStateComponent state) =>
        {
          GravityModifierSystem.Advance(modifiers);
          state = GravityModifierSystem.Recalculate(configuration, direction, modifiers);
        });
  }
}
