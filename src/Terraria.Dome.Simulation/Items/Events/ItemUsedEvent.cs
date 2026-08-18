using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ItemUsedEvent(
  PlayerHandle Player,
  ushort ItemType,
  int Slot,
  int HealthRestored,
  int ManaRestored,
  bool ConsumedMainItem,
  long Sequence,
  int ManaConsumed = 0,
  ushort BuffType = 0,
  int BuffDurationTicks = 0);
