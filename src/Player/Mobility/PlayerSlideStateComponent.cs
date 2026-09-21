namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Tile, Collision, Spatial, Item, and persistence
public sealed class PlayerSlideStateComponent
{
  public bool IsSliding { get; set; }

  public int Direction { get; set; }

  public bool IceSkate { get; set; }

  public int SpikedBootsLevel { get; set; }
}
