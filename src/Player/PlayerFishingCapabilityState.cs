namespace Terraria.Player;

public sealed class PlayerFishingCapabilityState
{
  public int BaseSkill { get; internal set; }

  public bool AllowsCrates { get; internal set; }

  public bool HasSonar { get; internal set; }

  public bool HasFishingLineProtection { get; internal set; }

  public bool HasBobberBonus { get; internal set; }

  public bool HasTackleBoxBonus { get; internal set; }

  public bool CanFishInLava { get; internal set; }

  public ContentId<ProjectileDefinition>? BobberOverrideType { get; internal set; }

  public int PolePower { get; internal set; }

  public int BaitPower { get; internal set; }

  public float LevelMultiplier { get; internal set; }

  public int EffectiveFishingLevel { get; internal set; }
}
