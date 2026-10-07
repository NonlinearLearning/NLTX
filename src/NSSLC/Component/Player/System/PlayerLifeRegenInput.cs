namespace Terraria.Player;

public readonly record struct PlayerLifeRegenInput(
  int LifeRegen,
  int LifeRegenCount,
  float LifeRegenTime,
  int StatLife,
  int StatLifeMax2,
  int SoulDrain,
  PlayerLifeRegenStatusFlags Statuses);
