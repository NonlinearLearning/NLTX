namespace Terraria.Player;

public readonly record struct PlayerSetMatchCompositionResult(
  int Head,
  int Body,
  int Legs,
  bool WearsRobe,
  bool SomethingSpecial);
