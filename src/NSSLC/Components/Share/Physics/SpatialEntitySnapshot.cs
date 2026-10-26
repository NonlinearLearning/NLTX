using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;

namespace Terraria.Physics;

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
      LiquidContactComponent? liquidContact = null)
  {
    Entity = entity;
    Location = location;
    Velocity = velocity;
    Collider = collider;
    Grounding = grounding ?? GroundingComponent.Airborne;
    LiquidContact = liquidContact ?? new LiquidContactComponent(LiquidContactFlags.None, 0);
  }

  public TEntity Entity { get; }

  public LocationComponent Location { get; }

  public VelocityComponent Velocity { get; }

  public ColliderComponent Collider { get; }

  public GroundingComponent Grounding { get; }

  public LiquidContactComponent LiquidContact { get; }

}
