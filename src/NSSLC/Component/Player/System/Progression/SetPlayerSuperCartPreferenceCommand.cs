namespace Terraria.Player.Progression;

public readonly record struct SetPlayerSuperCartPreferenceCommand(
  bool Enabled,
  PlayerProgressionCommandToken Token);
