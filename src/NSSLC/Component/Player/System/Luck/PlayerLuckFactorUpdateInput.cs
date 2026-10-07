namespace Terraria.Player.Luck;

public readonly record struct PlayerLuckFactorUpdateInput(
  int LadyBugLuckTimeLeft,
  float CoinLuck,
  int DayRate);
