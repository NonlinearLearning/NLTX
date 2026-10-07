using System.Numerics;
using Terraria.Content;

namespace Terraria.Npc;

public readonly record struct NpcAiInput(
  NpcDefinition Definition,
  int Slot,
  Vector2 Position,
  Vector2 Velocity,
  NpcAiStateComponent State,
  NpcAiEnvironmentSnapshot Environment,
  IReadOnlyList<NpcAiTargetSnapshot> Targets);
