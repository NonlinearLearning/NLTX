namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record TownAchievementEligibilityResult(
  bool RealEstateComplete,
  bool TownSlimesComplete,
  int RealEstateMissingCount,
  int TownSlimesMissingCount,
  bool AchievementNotificationDeferred);
