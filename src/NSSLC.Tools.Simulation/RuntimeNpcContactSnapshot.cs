using Terraria.Npc;
using Terraria.Relationships;

namespace Terraria.NonAuthoritative.SimulationHost;

internal readonly record struct RuntimeNpcContactSnapshot(
  EntityReference Reference,
  bool IsActive,
  bool IsHostile,
  int Damage,
  NpcTargetGeometrySnapshot Hitbox);
