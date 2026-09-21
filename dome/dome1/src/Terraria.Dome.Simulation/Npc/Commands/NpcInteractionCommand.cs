using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Commands;

public enum NpcInteractionKind
{
  Talk = 1,
  Shop = 2,
  Home = 3
}

public readonly record struct NpcInteractionCommand(
  PlayerHandle Player,
  NpcHandle Npc,
  NpcInteractionKind Kind,
  string SessionId,
  SimulationVector TargetPosition);
