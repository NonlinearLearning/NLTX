namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileDefinitionComponent(
  int ProjectileType,
  int BehaviorId,
  int DefaultDamage,
  int DefaultLifetimeTicks,
  ColliderComponent Collider,
  bool Friendly,
  bool Hostile);
