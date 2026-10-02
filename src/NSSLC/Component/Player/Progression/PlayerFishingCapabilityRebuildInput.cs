namespace Terraria.Player.Progression;

public readonly record struct PlayerFishingCapabilityRebuildInput(
  int FishingSkill,
  bool CratePotion,
  bool SonarPotion,
  bool AccFishingLine,
  bool AccFishingBobber,
  bool AccTackleBox,
  bool AccLavaFishing);
