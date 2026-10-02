namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Tile, Collision, Spatial, Projectile, network, and persistence
public sealed class PlayerPulleyStateComponent
{
  public byte Direction { get; set; }

  public bool IsActive { get; set; }
}
