namespace Terraria.Player.Movement;

// status: implemented-partial
// componentId: PLAYER.COMP.GRAVITY_AND_WATER_TRAVERSAL_STATE
// source-members: P08-1303..P08-1307
// crossSubsystemOwner: integration-review
public sealed class PlayerGravityAndWaterTraversalStateComponent
{
  public bool WaterWalk { get; internal set; }

  public bool WaterWalk2 { get; internal set; }

  // C03 owns gravity and movement parameters; this component owns direction and control facts.
  public int ForcedGravity { get; internal set; }

  public bool GravControl { get; internal set; }

  public bool GravControl2 { get; internal set; }
}
