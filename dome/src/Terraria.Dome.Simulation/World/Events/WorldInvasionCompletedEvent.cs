namespace Terraria.Dome.Simulation.World.Events;

using Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionCompletedEvent(
  int InvasionType,
  WorldInvasionClearFlag ClearFlag);
