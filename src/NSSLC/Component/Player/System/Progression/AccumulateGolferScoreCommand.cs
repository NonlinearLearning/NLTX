namespace Terraria.Player.Progression;

public readonly record struct AccumulateGolferScoreCommand(
  int Score,
  PlayerProgressionCommandToken Token);
