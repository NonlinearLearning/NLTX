namespace Terraria.Player;

public readonly record struct PlayerSetMatchResult(
  int MatchedSlot,
  bool Matched,
  bool SomethingSpecial);
