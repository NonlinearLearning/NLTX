using System.Collections.Generic;

namespace Terraria.Player.Environment;

public readonly record struct PlayerSunScorchInput(
  bool IsLocalPlayer,
  bool VampireSeed,
  int FeetTileY,
  double WorldSurface,
  bool DayTime,
  bool Raining,
  bool Eclipse,
  bool ZoneGraveyard,
  bool ZoneGlowshroom,
  bool HasMoonLordSkyIntensity,
  float MoonLordSkyIntensity,
  bool Wet,
  int SelectedItemType,
  bool MountActive,
  int MountType,
  bool ShouldShowInvisibleBlocksAndWalls,
  IReadOnlyList<PlayerSunScorchTileFact> TileFactsFromFeetUp,
  bool OnFire);
