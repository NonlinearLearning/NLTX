namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Tile, Collision, Mount, and Spatial ownership
public sealed class PlayerGroundTraversalStateComponent
{
  public bool StairFall { get; set; }

  public bool IsSloping { get; set; }

  public float AcceleratedRunSpeed { get; set; }

  public bool PowerRun { get; set; }

  public bool RunningOnSand { get; set; }
}
