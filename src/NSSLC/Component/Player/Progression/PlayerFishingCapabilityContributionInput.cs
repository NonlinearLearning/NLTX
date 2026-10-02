namespace Terraria.Player.Progression;

public readonly record struct PlayerFishingCapabilityContributionInput(
  int FishingSkillDelta,
  bool CratePotion,
  bool SonarPotion,
  bool AccFishingLine,
  bool AccFishingBobber,
  bool AccTackleBox,
  bool AccLavaFishing);
