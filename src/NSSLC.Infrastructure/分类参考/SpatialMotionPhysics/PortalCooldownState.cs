namespace Terraria.SpatialMotionPhysics;

public sealed class PortalCooldownState
{
  private readonly HashSet<int> _coolingEntities = new();

  public void Start(int entityId)
  {
    _coolingEntities.Add(entityId);
  }

  public bool IsCoolingDown(int entityId)
  {
    return _coolingEntities.Contains(entityId);
  }

  public void Clear(int entityId)
  {
    _coolingEntities.Remove(entityId);
  }
}
