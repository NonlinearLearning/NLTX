using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ChestItemReplicationSnapshot(
  int ChestId,
  byte Slot,
  ItemStack Stack,
  byte? Opener,
  long Revision);
