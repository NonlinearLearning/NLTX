namespace Terraria.Dome.Simulation.Player;

public readonly record struct PlayerInputEdges(
  bool WasUseItemPressed,
  bool WasUseItemReleased,
  bool WasUseTilePressed,
  bool WasUseTileReleased,
  bool WasDashPressed,
  bool WasDashReleased);
