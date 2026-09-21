namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: ZoneCorrupt, ZoneCrimson, ZoneHallow, ZoneJungle, ZoneSnow,
// ZoneGlowshroom, ZoneMeteor, ZoneGraveyard, ZoneDungeon, ZoneLihzhardTemple,
// ZoneGranite, ZoneMarble, ZoneSandstorm
// lifecycle: one spawn evaluation; not durable entity state
public readonly record struct NpcSpawnBiomeZoneEligibilitySnapshot(
  bool ZoneCorrupt,
  bool ZoneCrimson,
  bool ZoneHallow,
  bool ZoneJungle,
  bool ZoneSnow,
  bool ZoneGlowshroom,
  bool ZoneMeteor,
  bool ZoneGraveyard,
  bool ZoneDungeon,
  bool ZoneLihzhardTemple,
  bool ZoneGranite,
  bool ZoneMarble,
  bool ZoneSandstorm);
