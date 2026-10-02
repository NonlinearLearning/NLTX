namespace Terraria.Player.Movement;

// status: implemented-partial
// componentId: PLAYER.COMP.MOVEMENT_PHYSICS_STATE
// source-members: P08-1147..P08-1151
// crossSubsystemOwner: integration-review
public sealed class PlayerMovementPhysicsStateComponent
{
  // This baseline mirrors the Version4 instance initialization; the default owner remains under review.
  public float Gravity { get; internal set; } = 0.4f;

  public float MaxFallSpeed { get; internal set; } = 10f;

  public float MaxRunSpeed { get; internal set; } = 3f;

  public float RunAcceleration { get; internal set; } = 0.08f;

  public float RunSlowdown { get; internal set; } = 0.2f;
}
