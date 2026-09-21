namespace Terraria.Player.Combat;

// status: implemented
// componentId: PLAYER.COMP.ARMOR_AND_COMBAT_EFFECTS
// source-members: P08-1275..P08-1281
// crossSubsystemOwner: integration-review
public sealed class PlayerArmorAndCombatEffectsComponent
{
  public float Thorns { get; internal set; }

  public bool TurtleArmor { get; internal set; }

  public bool TurtleThorns { get; internal set; }

  public bool CactusThorns { get; internal set; }

  public bool SpiderArmor { get; internal set; }

  public bool AnglerSetSpawnReduction { get; internal set; }

  public bool VampireBurningInSunlight { get; internal set; }
}
