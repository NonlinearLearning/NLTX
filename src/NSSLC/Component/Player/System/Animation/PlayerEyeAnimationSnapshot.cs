namespace Terraria.Player.Animation;

public readonly record struct PlayerEyeAnimationSnapshot(
  PlayerEyeAnimationState State,
  int TimeInState,
  int EyeFrameToShow);
