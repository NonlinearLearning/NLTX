namespace Terraria.Dome.Simulation.Player.Events;

public readonly record struct PlayerRespawnedEvent(PlayerHandle Player, SimulationVector Spawn);
