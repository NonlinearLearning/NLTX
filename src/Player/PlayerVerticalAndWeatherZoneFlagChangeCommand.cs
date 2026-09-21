namespace Terraria.Player;

public readonly record struct PlayerVerticalAndWeatherZoneFlagChangeCommand(
  PlayerVerticalAndWeatherZoneFlag Flag,
  bool Enabled);
