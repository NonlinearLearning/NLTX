namespace Terraria.Player;

public readonly record struct PlayerActivityDecision(
  bool CountAfk,
  bool ResetAfkCounter,
  bool ResetKitingCounter);
