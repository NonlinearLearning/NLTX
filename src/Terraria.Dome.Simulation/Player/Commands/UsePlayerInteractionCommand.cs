using Terraria.Dome.Simulation.Player.Components;

namespace Terraria.Dome.Simulation.Player.Commands;

public readonly record struct UsePlayerInteractionCommand(
  PlayerHandle Player,
  int TargetId,
  PlayerInteractionMode Mode,
  SimulationVector TargetPosition = default);
