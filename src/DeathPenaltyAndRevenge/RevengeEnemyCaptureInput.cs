using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeEnemyCaptureInput(
  WorldPosition Position,
  float Width,
  float Height,
  bool IsBoss,
  bool HasNonRootRealLife,
  int Rarity,
  int CoinValue,
  NpcNetId SpawnNpcNetId,
  RevengeEnemyCaptureBounds WorldBounds);
