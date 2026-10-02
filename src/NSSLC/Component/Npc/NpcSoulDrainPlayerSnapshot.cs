using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcSoulDrainPlayerSnapshot(
  bool IsActive,
  bool IsDead,
  Vector2 Position,
  int SelectedItemType,
  int ItemAnimation);
