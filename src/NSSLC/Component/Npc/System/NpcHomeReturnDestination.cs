using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcHomeReturnDestination(
  Vector2 Position,
  int CandidateOffset);
