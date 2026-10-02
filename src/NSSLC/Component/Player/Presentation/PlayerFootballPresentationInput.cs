namespace Terraria.Player.Presentation;

public readonly record struct PlayerFootballPresentationInput(
  bool HasFootball,
  bool CanDrawFootball,
  bool IsFootballItemAnimating = false,
  bool ResetState = false);
