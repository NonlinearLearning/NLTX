namespace Terraria.Dome.Simulation.Commands;

public readonly record struct SpawnProjectileCommand(
  PlayerHandle Owner,
  float X,
  float Y,
  int Facing,
  int Damage,
  int LifetimeTicks,
  int ProjectileType = 1,
  int BehaviorId = 1,
  float InitialVelocityY = 0.0f,
  int MaximumPenetration = 1);
