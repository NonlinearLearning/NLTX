namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1406
// crossSubsystemOwner: tile interaction command and scheduler remain integration-review
public sealed class PlayerInteractionLockStateComponent
{
  public int LockTileInteractionsTimer { get; internal set; }
}
