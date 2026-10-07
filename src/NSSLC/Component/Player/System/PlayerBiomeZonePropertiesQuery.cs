namespace Terraria.Player;

public static class PlayerBiomeZonePropertiesQuery
{
  public static PlayerBiomeZonePropertiesSnapshot Evaluate(
    in PlayerZoneSnapshot input)
  {
    return new PlayerBiomeZonePropertiesSnapshot(
      input.ZoneDungeon,
      input.ZoneCorrupt,
      input.ZoneHallow,
      input.ZoneMeteor,
      input.ZoneJungle,
      input.ZoneSnow,
      input.ZoneCrimson,
      input.ZoneWaterCandle,
      input.ZonePeaceCandle,
      input.ZoneTowerSolar,
      input.ZoneTowerVortex,
      input.ZoneTowerNebula,
      input.ZoneTowerStardust,
      input.ZoneDesert,
      input.ZoneGlowshroom,
      input.ZoneUndergroundDesert);
  }
}
