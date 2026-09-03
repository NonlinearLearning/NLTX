namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcActivitySlotContributionInput(
  int NpcType,
  int LifeMaximum,
  int ReleaseOwner,
  float NpcSlotCost,
  bool IsSlimeRainActive,
  bool IsNpcActive,
  bool IsPlayerInActiveRange);
