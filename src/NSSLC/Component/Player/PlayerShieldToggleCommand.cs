namespace Terraria.Player;

public readonly record struct PlayerShieldToggleCommand(
  Guid CommandId,
  bool ShouldGuard);
