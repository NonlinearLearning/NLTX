namespace Terraria.Teleportation;

// Stores the short-lived teleport request guard.
// Position commits, visuals and network acknowledgements remain external effects.
public sealed class PlayerTeleportTransitionComponent
{
  public bool IsTeleporting { get; set; }
}
