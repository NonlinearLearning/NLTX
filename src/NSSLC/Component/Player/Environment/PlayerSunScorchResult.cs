namespace Terraria.Player.Environment;

public readonly record struct PlayerSunScorchResult(
  bool Updated,
  bool IsBurningInSunlight,
  int PreviousCounter,
  int CurrentCounter,
  float SizzleVolume,
  PlayerSunScorchEffectRequest RequestedEffects);
