using Arch.Core;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct DespawnEntityCommand(
  Entity Target,
  ProjectileTombstoneReason ProjectileReason = ProjectileTombstoneReason.None);
