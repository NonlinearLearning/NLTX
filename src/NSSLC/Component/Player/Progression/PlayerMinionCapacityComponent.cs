using System;
using System.Collections.Generic;
using Terraria.Relationships;

namespace Terraria.Player.Progression;

public sealed class PlayerMinionCapacityComponent
{
  private readonly EntityReference _owner;
  private readonly object _sync = new();
  private readonly HashSet<Guid> _appliedTokens = new();

  public PlayerMinionCapacityComponent(EntityReference owner)
  {
    if (owner.IsEmpty || owner.Scope != EntityReferenceScope.Player)
    {
      throw new ArgumentException("A player owner identity is required.", nameof(owner));
    }

    _owner = owner;
  }

  internal EntityReference Owner => _owner;

  internal object SyncRoot => _sync;

  public int MaxMinions { get; internal set; } = 1;

  public int NumMinions { get; internal set; }

  public float SlotsMinions { get; internal set; }

  internal bool HasApplied(Guid token)
  {
    return _appliedTokens.Contains(token);
  }

  internal bool TryMarkApplied(Guid token)
  {
    return _appliedTokens.Add(token);
  }

  internal void ClearAppliedTokens()
  {
    _appliedTokens.Clear();
  }
}
