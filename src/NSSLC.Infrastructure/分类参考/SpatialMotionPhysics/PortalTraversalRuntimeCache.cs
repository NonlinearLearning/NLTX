namespace Terraria.SpatialMotionPhysics;

public sealed class PortalTraversalRuntimeCache
{
  private readonly Dictionary<int, int> _portalPairs = new();

  public bool AnyPortalAtAll { get; private set; }

  public void SetPortalAvailable(bool available)
  {
    AnyPortalAtAll = available;
  }

  public void SetPortalPair(int sourcePortal, int destinationPortal)
  {
    _portalPairs[sourcePortal] = destinationPortal;
    AnyPortalAtAll = true;
  }

  public bool HasPair(int sourcePortal)
  {
    return _portalPairs.ContainsKey(sourcePortal);
  }
}
