namespace Terraria.Player;

public readonly record struct PlayerZoneFlagChangeCommand(
  PlayerZoneFlag Flag,
  bool Enabled);
