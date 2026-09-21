using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ItemDroppedEvent(
  int SourceEntityId,
  ushort ItemType,
  int Quantity,
  long Tick);
