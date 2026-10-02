using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeDiscouragementInput(
  int AiStyle,
  WorldPosition PlayerPosition,
  NpcTypeId DiscouragementNpcTypeId,
  NpcNetId SpawnNpcNetId,
  bool IsDayTime,
  bool IsEclipse,
  bool PlayerInUndergroundDesert,
  double WorldSurfaceTileY,
  bool AiStyle2IsDiscouraged,
  bool AiStyle3IsNotDiscouraged);
