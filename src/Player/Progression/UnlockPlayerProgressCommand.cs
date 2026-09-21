namespace Terraria.Player.Progression;

public readonly record struct UnlockPlayerProgressCommand(
  PlayerUnlockProgressionKind Progression,
  PlayerProgressionCommandToken Token);
