using Terraria.Share.Entities;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;

namespace Terraria.Share.Physics;

/// <summary>
/// Detached spatial state captured from one ECS entity for one simulation step.
/// </summary>
public readonly record struct SpatialEntitySnapshot<TEntity>
    where TEntity : notnull
{
  public SpatialEntitySnapshot(
      TEntity entity,
      LocationComponent location,
      VelocityComponent velocity,
      ColliderComponent collider,
      GroundingComponent? grounding = null,
      LiquidContactComponent? liquidContact = null,
      SubstepCapabilityComponent? substep = null)
  {
    Entity = entity;
    Location = location;
    Velocity = velocity;
    Collider = collider;
    Grounding = grounding ?? GroundingComponent.Airborne;
    LiquidContact = liquidContact ?? new LiquidContactComponent(LiquidContactFlags.None, 0);
    Substep = substep ?? SubstepCapabilityComponent.Single;
  }

  public TEntity Entity { get; }

  public LocationComponent Location { get; }

  public VelocityComponent Velocity { get; }

  public ColliderComponent Collider { get; }

  public GroundingComponent Grounding { get; }

  public LiquidContactComponent LiquidContact { get; }

  /// <summary>本次推进要切成的子步数量。</summary>
  public SubstepCapabilityComponent Substep { get; }

}
