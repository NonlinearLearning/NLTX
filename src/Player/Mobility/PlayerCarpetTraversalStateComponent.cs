namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Jump, Wing, Grapple, Mount, Spatial, and presentation
public sealed class PlayerCarpetTraversalStateComponent
{
  public bool HasCarpet { get; set; }

  public bool CanStart { get; set; }

  public int RemainingTime { get; set; }
}
