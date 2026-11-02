using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>Tick-scoped queue of validated damage requests awaiting the Combat commit step.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity instead of being handed to producers and
/// consumers by the host.
/// </remarks>
public sealed class CombatDamageRequestQueueComponent
{
  private readonly List<DamageRequest> _pending = [];

  public int PendingCount => _pending.Count;

  public void Enqueue(in DamageRequest request)
  {
    _pending.Add(request);
  }

  public IReadOnlyList<DamageRequest> DrainPending()
  {
    DamageRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(requests);
  }

  public void Clear()
  {
    _pending.Clear();
  }
}
