namespace Terraria.Player;

public readonly record struct PlayerStatusEffectTickResult(
  bool TimersAdvanced,
  IReadOnlyList<ContentId<BuffDefinition>> ExpiredEffects,
  int ActiveCount);
