using Arch.Core;

namespace Terraria.Dome.Simulation.Combat.Events;

public readonly record struct DamageRequestedEvent(
  Entity Projectile,
  Entity Target,
  int Amount,
  int ProjectileIdentity,
  int TargetIdentity);
