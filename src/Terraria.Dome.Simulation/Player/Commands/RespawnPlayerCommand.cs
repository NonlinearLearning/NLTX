namespace Terraria.Dome.Simulation.Player.Commands;

public readonly record struct RespawnPlayerCommand(PlayerHandle Player, SimulationVector Spawn);
