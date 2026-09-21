namespace Terraria.Teleportation;

// Stores short-lived Portal/Pylon traversal metadata and physics requests.
// Teleport commits and visual effects remain owned by their respective adapters.
public sealed class PlayerPortalTraversalComponent
{
  public int LastPortalColorIndex { get; set; }

  public int PortalPhysicsRemainingTicks { get; set; }

  public bool PortalPhysicsRequested { get; set; }

  public int LastPylonStyle { get; set; }
}
