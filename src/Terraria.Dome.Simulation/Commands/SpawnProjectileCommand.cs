using Arch.Core;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct SpawnProjectileCommand(
  Entity Owner,
  float X,
  float Y,
  int Facing,
  int Damage,
  int LifetimeTicks);
