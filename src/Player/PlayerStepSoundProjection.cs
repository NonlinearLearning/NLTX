namespace Terraria.Player;

public readonly record struct PlayerStepSoundProjection(
  int SoundType,
  int SoundStyle,
  int IntendedCooldown);
