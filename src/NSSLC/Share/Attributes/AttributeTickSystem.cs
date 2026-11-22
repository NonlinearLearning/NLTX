using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Attributes;

/// <summary>
/// Advances timed attribute modifiers for entities that carry an attribute set.
/// </summary>
/// <remarks>
/// This adapter only organizes an existing deterministic rule for Arch. Equipment, buffs and
/// combat systems remain responsible for adding or removing their own modifiers.
/// </remarks>
public sealed class AttributeTickSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  public AttributeTickSystem(ArchWorld world) : base(world) { }

  public override void Update(in EcsServerTickContext context)
  {
    QueryDescription query = new QueryDescription().WithAll<AttributeSetComponent>();
    context.World.Query(in query, static (ref AttributeSetComponent attributes) =>
    {
      AttributeSystem.Advance(attributes);
    });
  }
}
