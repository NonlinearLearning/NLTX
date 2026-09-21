namespace Terraria.WorldGeneration.Components;

public readonly record struct JungleRegionStructureSnapshot(
  long GenerationId,
  int ExtraBastStatueCount,
  int ExtraBastStatueCountMax,
  int JungleOriginX,
  int JungleMinX,
  int JungleMaxX,
  ushort JungleHut,
  bool MudWall);
