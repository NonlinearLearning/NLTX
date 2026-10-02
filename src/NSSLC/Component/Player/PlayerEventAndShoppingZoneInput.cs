using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerEventAndShoppingZoneInput(
  bool ZoneOldOneArmy,
  bool ZoneLihzhardTemple,
  bool ZoneGraveyard,
  bool ZoneShadowCandle,
  bool ZoneShimmer,
  bool ZoneDungeon,
  bool ZoneCorrupt,
  bool ZoneCrimson,
  bool ZoneGlowshroom,
  bool ZoneHallow,
  bool ZoneJungle,
  bool ZoneSnow,
  bool ZoneBeach,
  bool ZoneDesert,
  Vector2 Position,
  double WorldSurface);
