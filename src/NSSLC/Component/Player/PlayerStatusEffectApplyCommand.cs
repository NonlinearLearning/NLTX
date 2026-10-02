namespace Terraria.Player;

public readonly record struct PlayerStatusEffectApplyCommand(
  ContentId<BuffDefinition> EffectType,
  int DurationTicks);
