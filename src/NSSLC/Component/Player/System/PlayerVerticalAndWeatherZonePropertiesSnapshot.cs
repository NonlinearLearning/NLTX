namespace Terraria.Player;

public readonly record struct PlayerVerticalAndWeatherZonePropertiesSnapshot(
  bool ZoneSkyHeight,
  bool ZoneOverworldHeight,
  bool ZoneUnderworldHeight,
  bool ZoneBeach,
  bool ZoneRain,
  bool ZoneSandstorm);
