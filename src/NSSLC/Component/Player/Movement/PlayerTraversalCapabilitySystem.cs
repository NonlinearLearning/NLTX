using Terraria.Player.Environment;

namespace Terraria.Player.Movement;

// status: implemented-partial
// crossSubsystemOwner: P03 Mount and P07 Movement/Collision consumers need integration review
public sealed class PlayerTraversalCapabilitySystem
{
  public PlayerTraversalCapabilitySnapshot CreateSnapshot(
    PlayerMovementPhysicsStateComponent physics,
    PlayerGravityAndWaterTraversalStateComponent gravity,
    PlayerEnvironmentMobilityStateComponent mobility)
  {
    ArgumentNullException.ThrowIfNull(physics);
    ArgumentNullException.ThrowIfNull(gravity);
    ArgumentNullException.ThrowIfNull(mobility);

    return new PlayerTraversalCapabilitySnapshot(
      physics.Gravity,
      physics.MaxFallSpeed,
      physics.MaxRunSpeed,
      physics.RunAcceleration,
      physics.RunSlowdown,
      gravity.WaterWalk,
      gravity.WaterWalk2,
      gravity.ForcedGravity,
      gravity.GravControl,
      gravity.GravControl2,
      mobility.CanFloatInWater,
      mobility.JumpBoost,
      mobility.FrogLegJumpBoost,
      mobility.NoFallDamage,
      mobility.SwimTime,
      mobility.LavaImmune,
      mobility.Gills,
      mobility.SlowFall);
  }
}
