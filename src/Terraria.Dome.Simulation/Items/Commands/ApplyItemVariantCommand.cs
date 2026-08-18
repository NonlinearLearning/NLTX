using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct ApplyItemVariantCommand(
  PlayerHandle Player,
  int SourceSlot,
  ItemVariantDefinition Variant,
  long Sequence = 0);
