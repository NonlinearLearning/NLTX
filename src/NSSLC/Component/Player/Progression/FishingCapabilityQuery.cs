namespace Terraria.Player.Progression;

public static class FishingCapabilityQuery
{
  public static int GetFishingSkill(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.FishingSkill;
  }

  public static bool HasCratePotion(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.CratePotion;
  }

  public static bool HasSonarPotion(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.SonarPotion;
  }

  public static bool HasFishingLineProtection(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.AccFishingLine;
  }

  public static bool HasBobberBonus(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.AccFishingBobber;
  }

  public static bool HasTackleBoxBonus(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.AccTackleBox;
  }

  public static bool CanFishInLava(PlayerFishingCapabilitySnapshotComponent snapshot)
  {
    return snapshot.AccLavaFishing;
  }
}
