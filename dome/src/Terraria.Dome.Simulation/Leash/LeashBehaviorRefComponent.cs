namespace Terraria.Dome.Simulation.Leash;

public readonly record struct LeashBehaviorRefComponent(
  int BehaviorId,
  int AnchorStyle,
  int? NpcType,
  int? ProjectileType,
  bool IsAquatic);
