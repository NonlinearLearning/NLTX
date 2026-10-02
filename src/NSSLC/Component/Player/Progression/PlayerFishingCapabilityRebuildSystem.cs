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

  // Call after Reset for each source contribution; repeated calls add the contribution again.
  public static void Contribute(
    in PlayerFishingCapabilityContributionInput contribution,
    ref PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    unchecked
    {
      snapshot.FishingSkill += contribution.FishingSkillDelta;
    }

    snapshot.CratePotion |= contribution.CratePotion;
    snapshot.SonarPotion |= contribution.SonarPotion;
    snapshot.AccFishingLine |= contribution.AccFishingLine;
    snapshot.AccFishingBobber |= contribution.AccFishingBobber;
    snapshot.AccTackleBox |= contribution.AccTackleBox;
    snapshot.AccLavaFishing |= contribution.AccLavaFishing;
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
