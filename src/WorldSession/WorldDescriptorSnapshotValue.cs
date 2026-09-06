using System;

namespace Terraria.WorldSession.Components;

public readonly record struct WorldDescriptorSnapshotValue(
  int WorldId,
  Guid UniqueId,
  string Name,
  string SeedText,
  ulong WorldGeneratorVersion,
  int SizeX,
  int SizeY,
  WorldBounds Bounds,
  double SurfaceLayer,
  double RockLayer,
  int SpawnTileX,
  int SpawnTileY,
  int DungeonTileX,
  int DungeonTileY);
