using Terraria.Npc;
using Terraria.Relationships;
using Terraria.WorldStorage;
using NpcRuntimeSlot = Terraria.Npc.NpcSlot;

namespace Terraria.NonAuthoritative.SimulationHost;

internal readonly record struct RuntimeNpcTrainingDummyBinding(
  EntityReference Reference,
  RuntimeEntityHandle RuntimeHandle,
  NpcRuntimeSlot Slot,
  uint SlotGeneration,
  TileCoordinate Anchor);
