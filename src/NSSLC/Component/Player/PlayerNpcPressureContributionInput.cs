namespace Terraria.Player;

public readonly record struct PlayerNpcPressureContributionInput(
  int NpcType,
  int LifeMaximum,
  int ReleaseOwner,
  float NpcSlotCost,
  bool IsSlimeRainActive,
  bool IsSlimeRainNpc,
  bool IsNpcActive,
  bool IsPlayerInActiveRange);
