namespace Terraria.Player.Progression;

public sealed class PlayerFishingCapabilitySnapshotComponent
{
  public int FishingSkill { get; internal set; }

  public bool CratePotion { get; internal set; }

  public bool SonarPotion { get; internal set; }

  public bool AccFishingLine { get; internal set; }

  public bool AccFishingBobber { get; internal set; }

  public bool AccTackleBox { get; internal set; }

  public bool AccLavaFishing { get; internal set; }
}
