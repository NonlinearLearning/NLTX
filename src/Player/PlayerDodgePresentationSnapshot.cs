namespace Terraria.Player;

public readonly record struct PlayerDodgePresentationSnapshot(
  int BrainOfConfusionDodgeAnimationCounter,
  bool ShadowDodgeActive);
