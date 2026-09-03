namespace Terraria.Dome.Simulation.Player.Events;

public readonly record struct PlayerDamagedEvent(
  PlayerHandle Player,
  int RequestedAmount,
  int AppliedAmount,
  int RemainingHealth);
