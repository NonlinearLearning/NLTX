using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileBehaviorSystem
{
  private readonly IReadOnlyDictionary<int, IProjectileBehavior> _behaviors;

  public ProjectileBehaviorSystem(IEnumerable<IProjectileBehavior> behaviors)
  {
    ArgumentNullException.ThrowIfNull(behaviors);
    Dictionary<int, IProjectileBehavior> indexed = new();
    foreach (IProjectileBehavior behavior in behaviors)
    {
      if (!indexed.TryAdd(behavior.BehaviorId, behavior))
      {
        throw new ArgumentException("Projectile behavior IDs must be unique.", nameof(behaviors));
      }
    }

    _behaviors = indexed;
  }

  public static ProjectileBehaviorSystem CreateDefault()
  {
    return new ProjectileBehaviorSystem([
      new LinearProjectileBehavior(),
      new GravityProjectileBehavior(),
      new LegacyAiStyle2ProjectileBehavior(),
      new LegacyAiStyle2DelayedProjectileBehavior(),
      new LegacyAiStyle2ImmediateGravityProjectileBehavior(),
      new LegacyAiStyle2FiveTickGravityProjectileBehavior(),
      new LegacyAiStyle2SixtyTickGravityProjectileBehavior(),
      new LegacyAiStyle2RandomFrameProjectileBehavior(),
      new LegacyAiStyle2EighteenTickGravityProjectileBehavior(),
      new LegacyAiStyle2SixteenTickGravityProjectileBehavior(),
      new LegacyAiStyle29ParentProjectileBehavior(),
      new LegacyAiStyle29ChildProjectileBehavior(),
      new LegacyAiStyle2Type162ProjectileBehavior(),
      new LegacyAiStyle49Type281ProjectileBehavior(),
      new LegacyAiStyle2Type166ProjectileBehavior(),
      new LegacyAiStyle2Type304ProjectileBehavior(),
      new LegacyAiStyle2StatusEffectProjectileBehavior(),
      new LegacyAiStyle2HitStatusProjectileBehavior(),
      new LegacyAiStyle190ProjectileBehavior(),
      new LegacyAiStyle17ProjectileBehavior(),
      new LegacyType607ProjectileBehavior()]);
  }

  public bool TryAdvance(
    Entity entity,
    Arch.Core.World world,
    int tick,
    out int behaviorId)
  {
    ArgumentNullException.ThrowIfNull(world);
    behaviorId = 0;
    if (!world.IsAlive(entity))
    {
      return false;
    }

    ref ProjectileBehaviorComponent behavior = ref world.Get<ProjectileBehaviorComponent>(entity);
    behaviorId = behavior.BehaviorId;
    if (!_behaviors.TryGetValue(behavior.BehaviorId, out IProjectileBehavior? implementation))
    {
      return false;
    }

    ref TransformComponent transform = ref world.Get<TransformComponent>(entity);
    ref VelocityComponent velocity = ref world.Get<VelocityComponent>(entity);
    return implementation.TryAdvance(ref transform, ref velocity, ref behavior, tick);
  }
}
