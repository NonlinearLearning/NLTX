using System.Collections.Generic;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Tick-scoped queue of requests to restore one entity to full life.</summary>
/// <remarks>
/// Player owns the death and respawn decision but does not own <see cref="LifeComponent"/>. It
/// states the outcome it wants here and the Combat domain performs the write, so
/// <c>DamageCommitSystem</c> stays the only writer of life outside respawn.
/// </remarks>
public sealed class LifeRestoreRequestQueueComponent
{
  private readonly List<LifeRestoreRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(EntityRuntimeHandle target)
    => _pending.Add(new LifeRestoreRequest(target, amount: 0, sourceId: 0, tick: 0));

  public void Enqueue(in LifeRestoreRequest request) => _pending.Add(request);

  public IReadOnlyList<LifeRestoreRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    LifeRestoreRequest[] pending = _pending.ToArray();
    _pending.Clear();
    return pending;
  }

  public void Clear() => _pending.Clear();
}

/// <summary>One bounded healing intent emitted by a behavior system.</summary>
public readonly record struct LifeRestoreRequest
{
  public LifeRestoreRequest(EntityRuntimeHandle target, int amount, long sourceId, long tick)
  {
    if (!target.IsValid || amount < 0 || sourceId < 0 || tick < 0 ||
        (amount == 0) != (sourceId == 0))
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    Target = target;
    Amount = amount;
    SourceId = sourceId;
    Tick = tick;
  }

  public EntityRuntimeHandle Target { get; }

  public int Amount { get; }

  public long SourceId { get; }

  public long Tick { get; }
}
