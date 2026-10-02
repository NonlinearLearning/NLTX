namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ENVIRONMENT_MOBILITY_STATE
// source-members: P08-1240..P08-1245, P08-1258..P08-1260, P08-1266..P08-1268
// crossSubsystemOwner: integration-review
public sealed class PlayerEnvironmentMobilityStateComponent
{
  public bool CanFloatInWater { get; internal set; }

  public bool HasFloatingTube { get; internal set; }

  public bool FrogLegJumpBoost { get; internal set; }

  public bool SkyStoneEffects { get; internal set; }

  public bool SpawnMax { get; internal set; }

  public int BlockRange { get; internal set; }

  public bool JumpBoost { get; internal set; }

  public bool NoFallDamage { get; internal set; }

  public int SwimTime { get; internal set; }

  public bool LavaImmune { get; internal set; }

  public bool Gills { get; internal set; }

  public bool SlowFall { get; internal set; }
}
