namespace Terraria.Dome.Simulation.Player.Events;

public readonly record struct PlayerDiedEvent(PlayerHandle Player, int RespawnDelayTicks);
