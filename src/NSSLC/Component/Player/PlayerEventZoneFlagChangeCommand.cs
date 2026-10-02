namespace Terraria.Player;

public readonly record struct PlayerEventZoneFlagChangeCommand(
  PlayerEventZoneFlag Flag,
  bool Enabled);
