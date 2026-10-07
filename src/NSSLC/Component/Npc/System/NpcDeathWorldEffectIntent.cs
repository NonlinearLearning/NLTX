using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcDeathWorldEffectIntent(
  NpcDeathWorldEffectKind Kind,
  NpcInstanceId SourceNpcInstanceId,
  NpcTypeId SpawnNpcType,
  Vector2 OriginCenter,
  Vector2? FixedPositionPixels,
  int SpawnCount,
  int RandomOffsetRadiusInTiles,
  int SearchAttemptsPerSpawn,
  int WorldBottomMarginInTiles,
  bool RequestReplicationSyncAfterSpawn);
