namespace Terraria.Player;

public readonly record struct PlayerZoneSnapshot(
  bool ZoneDungeon,
  bool ZoneCorrupt,
  bool ZoneHallow,
  bool ZoneMeteor,
  bool ZoneJungle,
  bool ZoneSnow,
  bool ZoneCrimson,
  bool ZoneWaterCandle,
  bool ZonePeaceCandle,
  bool ZoneTowerSolar,
  bool ZoneTowerVortex,
  bool ZoneTowerNebula,
  bool ZoneTowerStardust,
  bool ZoneDesert,
  bool ZoneGlowshroom,
  bool ZoneUndergroundDesert);
