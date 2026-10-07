namespace Terraria.Player;

public readonly record struct PlayerManaStatusInput(
  bool ManaSickBuffActive,
  int ManaSickBuffTime,
  int ManaRegenBonus,
  float ManaRegenDelayBonus);
