using Arch.Core;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct BounceProjectileCommand(
  Entity Target,
  bool ReflectHorizontal,
  bool ReflectVertical,
  float SafeX,
  float SafeY);
