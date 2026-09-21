using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct ApplyItemPrefixCommand(
  PlayerHandle Player,
  int SourceSlot,
  ushort PrefixId,
  long Sequence = 0);
