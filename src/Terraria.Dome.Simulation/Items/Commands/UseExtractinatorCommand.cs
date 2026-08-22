using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct UseExtractinatorCommand(
  PlayerHandle Player,
  int SourceSlot,
  int TargetX,
  int TargetY,
  long Sequence = 0);
