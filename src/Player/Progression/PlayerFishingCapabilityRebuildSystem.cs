namespace Terraria.Player.Progression;

public static class PlayerFishingCapabilityRebuildSystem
{
  public static void Rebuild(
    in PlayerFishingCapabilityRebuildInput input,
    ref PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    snapshot.FishingSkill = input.FishingSkill;
    snapshot.CratePotion = input.CratePotion;
    snapshot.SonarPotion = input.SonarPotion;
    snapshot.AccFishingLine = input.AccFishingLine;
    snapshot.AccFishingBobber = input.AccFishingBobber;
    snapshot.AccTackleBox = input.AccTackleBox;
    snapshot.AccLavaFishing = input.AccLavaFishing;
  }

  public static void Reset(ref PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    snapshot.FishingSkill = 0;
    snapshot.CratePotion = false;
    snapshot.SonarPotion = false;
    snapshot.AccFishingLine = false;
    snapshot.AccFishingBobber = false;
    snapshot.AccTackleBox = false;
    snapshot.AccLavaFishing = false;
  }
}
