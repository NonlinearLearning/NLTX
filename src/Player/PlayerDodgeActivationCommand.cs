namespace Terraria.Player;

public readonly record struct PlayerDodgeActivationCommand(
  Guid CommandId,
  long SourceRevision,
  int ShadowDodgeTimer,
  int AnimationTicks);
