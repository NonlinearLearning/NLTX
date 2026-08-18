using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public readonly record struct ProjectileDefinition(
  int ProjectileType,
  int BehaviorId,
  int Damage,
  int LifetimeTicks,
  ColliderComponent Collider,
  bool Friendly,
  bool Hostile,
  int MaximumPenetration);
