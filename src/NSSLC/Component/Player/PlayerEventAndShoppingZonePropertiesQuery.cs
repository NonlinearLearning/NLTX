namespace Terraria.Player;

public static class PlayerEventAndShoppingZonePropertiesQuery
{
  public static PlayerEventAndShoppingZonePropertiesSnapshot Evaluate(
    in PlayerEventAndShoppingZoneInput input)
  {
    bool anyBiome = input.ZoneDungeon ||
      input.ZoneCorrupt ||
      input.ZoneCrimson ||
      input.ZoneGlowshroom ||
      input.ZoneHallow ||
      input.ZoneJungle ||
      input.ZoneSnow ||
      input.ZoneBeach ||
      input.ZoneDesert;
    bool belowSurface = input.Position.Y > input.WorldSurface * 16.0;

    return new PlayerEventAndShoppingZonePropertiesSnapshot(
      input.ZoneOldOneArmy,
      input.ZoneLihzhardTemple,
      input.ZoneGraveyard,
      input.ZoneShadowCandle,
      input.ZoneShimmer,
      anyBiome,
      belowSurface);
  }
}
