using System.Collections.Generic;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Tick-scoped queue of requests to restore one entity to full life.</summary>
/// <remarks>
/// Player owns the death and respawn decision but does not own <see cref="LifeComponent"/>. It
/// states the outcome it wants here and the Combat domain performs the write, so
/// <c>DamageCommitSystem</c> stays the only writer of life outside respawn.
/// </remarks>
public sealed class LifeRestoreRequestQueueComponent
{
  private readonly List<EntityRuntimeHandle> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(EntityRuntimeHandle target) => _pending.Add(target);

  public IReadOnlyList<EntityRuntimeHandle> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    EntityRuntimeHandle[] pending = _pending.ToArray();
    _pending.Clear();
    return pending;
  }

  public void Clear() => _pending.Clear();
}
