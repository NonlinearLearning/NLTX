namespace Terraria.Player;

public readonly record struct PlayerChannelCancellationExpectation(
  int ProjectileTypeExpected,
  int ProjectileIndexExpected);
