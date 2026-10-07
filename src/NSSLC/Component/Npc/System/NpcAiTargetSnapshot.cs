using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcAiTargetSnapshot(
  int Slot,
  Vector2 Center,
  Vector2 Velocity,
  bool IsActive,
  bool IsDead) {
  public NpcTargetGeometrySnapshot Geometry { get; init; }

  public NpcAiTargetSnapshot(
    int slot,
    NpcTargetGeometrySnapshot geometry,
    Vector2 velocity,
    bool isActive,
    bool isDead)
    : this(slot, geometry.Center, velocity, isActive, isDead) {
    Geometry = geometry;
  }

  public bool IsLiving => IsActive && !IsDead;
}
