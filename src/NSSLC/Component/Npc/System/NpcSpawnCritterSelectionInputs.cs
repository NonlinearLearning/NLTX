namespace Terraria.Npc;

public readonly record struct NpcSpawnCritterSelectionInputs(
  bool RemixWorld,
  bool WaterTile,
  int SpawnTileX,
  int SpawnTileY,
  double WorldSurface,
  int GoldCritterChance,
  int GnomeChance,
  bool Halloween,
  bool Christmas,
  bool BirthdayPartyActive);
