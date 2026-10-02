namespace Terraria.Player.Mobility;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Inventory, Item, Projectile, network, and persistence
public sealed class PlayerRopeStateComponent
{
  public int RopeCount { get; set; }

  public bool HasCordage { get; set; }

  public int SelectedGem { get; set; } = -1;

  public int GemScanCounter { get; set; }

  // Version4 BitsByte storage is represented by its byte payload until the shared value type is available.
  public byte OwnedLargeGems { get; set; }
}
