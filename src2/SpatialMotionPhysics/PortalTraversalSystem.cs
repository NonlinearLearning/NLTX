using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class PortalTraversalSystem
{
  private readonly PortalGeometryDefinitionCatalog _catalog;
  private readonly PortalTraversalRuntimeCache _cache;
  private readonly PortalCooldownState _cooldown;

  public PortalTraversalSystem(
    PortalGeometryDefinitionCatalog catalog,
    PortalTraversalRuntimeCache cache,
    PortalCooldownState cooldown)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    _cooldown = cooldown ?? throw new ArgumentNullException(nameof(cooldown));
  }

  public PortalTraversalCandidate? TryCreateCandidate(
    int entityId,
    Vector2 destination,
    Vector2 velocity)
  {
    if (!_cache.AnyPortalAtAll ||
      !_cache.HasPair(0) ||
      _cooldown.IsCoolingDown(entityId) ||
      _catalog.PortalsPerPerson <= 0)
    {
      return null;
    }

    return new PortalTraversalCandidate(entityId, destination, velocity);
  }

  public void Commit(
    EntityMotionState motion,
    PortalTraversalCandidate candidate)
  {
    ArgumentNullException.ThrowIfNull(motion);
    motion.Position = candidate.Position;
    motion.Velocity = candidate.Velocity;
    _cooldown.Start(candidate.EntityId);
  }
}
