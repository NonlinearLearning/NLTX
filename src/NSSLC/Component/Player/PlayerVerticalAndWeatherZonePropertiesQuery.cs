namespace Terraria.Player;

public static class PlayerVerticalAndWeatherZonePropertiesQuery
{
  public static PlayerVerticalAndWeatherZonePropertiesSnapshot Evaluate(
    in PlayerVerticalAndWeatherZoneInput input)
  {
    return new PlayerVerticalAndWeatherZonePropertiesSnapshot(
      input.ZoneSkyHeight,
      input.ZoneOverworldHeight,
      input.ZoneUnderworldHeight,
      input.ZoneBeach,
      input.ZoneRain,
      input.ZoneSandstorm);
  }
}
