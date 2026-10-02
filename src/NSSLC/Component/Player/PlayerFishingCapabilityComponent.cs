namespace Terraria.Player;

public sealed class PlayerFishingCapabilityComponent
{
  public int BaseSkill { get; set; }

  public bool AllowsCrates { get; set; }

  public bool HasSonar { get; set; }

  public bool HasFishingLineProtection { get; set; }

  public bool HasBobberBonus { get; set; }

  public bool HasTackleBoxBonus { get; set; }

  public bool CanFishInLava { get; set; }

  public ContentId<ProjectileDefinition>? BobberOverrideType { get; set; }

  public int EffectiveFishingLevel { get; set; }
}
