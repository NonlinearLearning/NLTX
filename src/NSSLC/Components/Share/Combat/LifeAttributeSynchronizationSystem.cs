using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Attributes;
using Terraria.Entity;

namespace Terraria.Combat;

/// <summary>
/// Synchronizes calculated capacity attributes into their dedicated resource owners.
/// </summary>
/// <remarks>
/// Attributes remain the source of derived capacities. This system is the only bridge that updates
/// LifeComponent.Maximum and clamps a ShieldPoolComponent after a capacity change; it never writes
/// current life from an attribute calculation.
/// </remarks>
public sealed class LifeAttributeSynchronizationSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<LifeComponent, AttributeSetComponent>();

  public LifeAttributeSynchronizationSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    ArchWorld world = context.World;
    world.Query(
        in _query,
        (ArchEntity entity,
            ref LifeComponent life,
            ref AttributeSetComponent attributes) =>
        {
          int maximumLife = ResolveCapacity(
              attributes,
              AttributeKind.MaximumLife,
              life.Maximum);
          if (maximumLife != life.Maximum)
          {
            life = life.WithMaximum(maximumLife);
          }

          if (world.Has<ShieldPoolComponent>(entity))
          {
            int maximumShield = AttributeCombatCalculator.ResolveMaxShield(attributes);
            ShieldPoolComponent shield = world.Get<ShieldPoolComponent>(entity);
            shield.SetCurrent(Math.Min(shield.Current, maximumShield));
          }
        });
  }

  private static int ResolveCapacity(
      AttributeSetComponent attributes,
      AttributeKind attribute,
      int fallback)
  {
    float value = AttributeCalculator.Resolve(attributes, attribute, fallback);
    if (!float.IsFinite(value) || value <= 0f)
    {
      return fallback;
    }

    return value >= int.MaxValue ? int.MaxValue : Math.Max(1, (int)MathF.Floor(value));
  }
}
