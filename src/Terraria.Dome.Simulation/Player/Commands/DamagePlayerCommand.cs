namespace Terraria.Dome.Simulation.Player.Commands;

public readonly record struct DamagePlayerCommand(PlayerHandle Player, int Amount);
