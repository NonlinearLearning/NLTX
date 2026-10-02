using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcDeathPhaseInput(
  NpcTypeId NpcType,
  bool IsActive,
  bool IsLifeOwner,
  int CurrentLife,
  NpcAiStateComponent Ai,
  Vector2 Center,
  bool IsGoodWorld = false,
  float? BottomY = null,
  NpcDeathClothierSkeletronContext? ClothierSkeletronContext = null,
  NpcInstanceId SourceNpcInstanceId = default);
