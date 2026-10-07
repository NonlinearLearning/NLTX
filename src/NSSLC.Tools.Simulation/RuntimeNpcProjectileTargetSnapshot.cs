using Terraria.Npc;
using Terraria.Relationships;
using Terraria.SpatialSimulation.Components;

namespace Terraria.NonAuthoritative.SimulationHost;

internal readonly record struct RuntimeNpcProjectileTargetSnapshot(
  EntityReference Reference,
  RuntimeEntityHandle RuntimeHandle,
  NpcSlot Slot,
  uint SlotGeneration,
  int TypeId,
  int NetId,
  bool IsActive,
  bool Friendly,
  bool IsTownNpc,
  int AiStyle,
  float Ai2,
  MovementStateComponent Movement,
  NpcTargetGeometrySnapshot Hitbox);
