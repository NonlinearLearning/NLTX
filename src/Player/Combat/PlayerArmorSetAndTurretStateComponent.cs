namespace Terraria.Player.Combat;

// status: implemented-partial
// componentId: PLAYER.COMP.ARMOR_SET_AND_TURRET_STATE
// source-members: P08-1284..P08-1299, P08-1302
// excluded-members: P08-1300..P08-1301 (existing PlayerAbilityComponent owner)
// crossSubsystemOwner: integration-review
public sealed class PlayerArmorSetAndTurretStateComponent
{
  public bool SetSolar { get; internal set; }

  public bool SetVortex { get; internal set; }

  public bool SetNebula { get; internal set; }

  public int NebulaCooldown { get; internal set; }

  public bool SetStardust { get; internal set; }

  public bool SetForbidden { get; internal set; }

  public bool SetForbiddenCooldownLocked { get; internal set; }

  public bool SetChlorophyte { get; internal set; }

  public bool SetSquireTierThree { get; internal set; }

  public bool SetHuntressTierThree { get; internal set; }

  public bool SetApprenticeTierThree { get; internal set; }

  public bool SetMonkTierThree { get; internal set; }

  public bool SetSquireTierTwo { get; internal set; }

  public bool SetHuntressTierTwo { get; internal set; }

  public bool SetApprenticeTierTwo { get; internal set; }

  public bool SetMonkTierTwo { get; internal set; }

  public bool VortexStealthActive { get; internal set; }
}
