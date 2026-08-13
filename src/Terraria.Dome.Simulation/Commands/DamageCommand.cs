using Arch.Core;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct DamageCommand(Entity Source, Entity Target, int Amount);
