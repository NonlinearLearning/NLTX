using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcDeathPhaseContext(
  NpcAiStateComponent Ai,
  Vector2 Center,
  bool IsLifeOwner,
  bool IsGoodWorld,
  float? BottomY,
  NpcDeathClothierSkeletronContext? ClothierSkeletronContext = null,
  NpcInstanceId SourceNpcInstanceId = default,
  int NetMode = 0);
