namespace Terraria.Player.Movement;

public readonly record struct PlayerTraversalCapabilitySnapshot(
  float Gravity,
  float MaxFallSpeed,
  float MaxRunSpeed,
  float RunAcceleration,
  float RunSlowdown,
  bool WaterWalk,
  bool WaterWalk2,
  int ForcedGravity,
  bool GravControl,
  bool GravControl2,
  bool CanFloatInWater,
  bool JumpBoost,
  bool FrogLegJumpBoost,
  bool NoFallDamage,
  int SwimTime,
  bool LavaImmune,
  bool Gills,
  bool SlowFall);
